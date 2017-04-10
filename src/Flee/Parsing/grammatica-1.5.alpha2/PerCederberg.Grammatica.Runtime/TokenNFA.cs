using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flee.Parsing.grammatica_1._5.alpha2.PerCederberg.Grammatica.Runtime
{
    /**
    * A non-deterministic finite state automaton (NFA) for matching
    * tokens. It supports both fixed strings and simple regular
    * expressions, but should perform similar to a DFA due to highly
    * optimized data structures and tuning. The memory footprint during
    * matching should be near zero, since no heap memory is allocated
    * unless the pre-allocated queues need to be enlarged. The NFA also
    * does not use recursion, but iterates in a loop instead.
    */
    internal class TokenNFA
    {
        private readonly NFAState[] _initialChar = new NFAState[128];
        private readonly NFAState _initial = new NFAState();
        private readonly NFAStateQueue _queue = new NFAStateQueue();

        public void AddTextMatch(string str, bool ignoreCase, TokenPattern value)
        {
            NFAState state;
            char ch = str[0];

            if (ch < 128 && !ignoreCase)
            {
                state = _initialChar[ch];
                if (state == null)
                {
                    state = _initialChar[ch] = new NFAState();
                }
            }
            else
            {
                state = _initial.AddOut(ch, ignoreCase, null);
            }
            for (int i = 1; i < str.Length; i++)
            {
                state = state.AddOut(str[i], ignoreCase, null);
            }
            state.Value = value;
        }

        public void AddRegExpMatch(string pattern,
                                   bool ignoreCase,
                                   TokenPattern value)
        {
            TokenRegExpParser parser = new TokenRegExpParser(pattern, ignoreCase);
            string debug = "DFA regexp; " + parser.GetDebugInfo();

            var isAscii = parser.Start.IsAsciiOutgoing();
            for (int i = 0; isAscii && i < 128; i++)
            {
                bool match = false;
                for (int j = 0; j < parser.Start.Outgoing.Length; j++)
                {
                    if (parser.Start.Outgoing[j].Match((char)i))
                    {
                        if (match)
                        {
                            isAscii = false;
                            break;
                        }
                        match = true;
                    }
                }
                if (match && _initialChar[i] != null)
                {
                    isAscii = false;
                }
            }
            if (parser.Start.Incoming.Length > 0)
            {
                _initial.AddOut(new NFAEpsilonTransition(parser.Start));
                debug += ", uses initial epsilon";
            }
            else if (isAscii && !ignoreCase)
            {
                for (int i = 0; isAscii && i < 128; i++)
                {
                    for (int j = 0; j < parser.Start.Outgoing.Length; j++)
                    {
                        if (parser.Start.Outgoing[j].Match((char)i))
                        {
                            _initialChar[i] = parser.Start.Outgoing[j].State;
                        }
                    }
                }
                debug += ", uses ASCII lookup";
            }
            else
            {
                parser.Start.MergeInto(_initial);
                debug += ", uses initial state";
            }
            parser.End.Value = value;
            value.DebugInfo = debug;
        }

        public int Match(ReaderBuffer buffer, TokenMatch match)
        {
            int length = 0;
            int pos = 1;
            NFAState state;

            // The first step of the match loop has been unrolled and
            // optimized for performance below.
            this._queue.Clear();
            var peekChar = buffer.Peek(0);
            if (0 <= peekChar && peekChar < 128)
            {
                state = this._initialChar[peekChar];
                if (state != null)
                {
                    this._queue.AddLast(state);
                }
            }
            if (peekChar >= 0)
            {
                this._initial.MatchTransitions((char)peekChar, this._queue, true);
            }
            this._queue.MarkEnd();
            peekChar = buffer.Peek(1);

            // The remaining match loop processes all subsequent states
            while (!this._queue.Empty)
            {
                if (this._queue.Marked)
                {
                    pos++;
                    peekChar = buffer.Peek(pos);
                    this._queue.MarkEnd();
                }
                state = this._queue.RemoveFirst();
                if (state.Value != null)
                {
                    match.Update(pos, state.Value);
                }
                if (peekChar >= 0)
                {
                    state.MatchTransitions((char)peekChar, this._queue, false);
                }
            }
            return length;
        }
    }


    /**
     * An NFA state. The NFA consists of a series of states, each
     * having zero or more transitions to other states.
     */
    internal class NFAState
    {
        internal TokenPattern Value = null;
        internal NFATransition[] Incoming = new NFATransition[0];
        internal NFATransition[] Outgoing = new NFATransition[0];
        internal bool EpsilonOut = false;

        public bool HasTransitions()
        {
            return Incoming.Length > 0 || Outgoing.Length > 0;
        }
        public bool IsAsciiOutgoing()
        {
            for (int i = 0; i < Outgoing.Length; i++)
            {
                if (!Outgoing[i].IsAscii())
                {
                    return false;
                }
            }
            return true;
        }

        public void AddIn(NFATransition trans)
        {
            Array.Resize(ref Incoming, Incoming.Length + 1);
            Incoming[Incoming.Length - 1] = trans;
        }

        public NFAState AddOut(char ch, bool ignoreCase, NFAState state)
        {
            if (ignoreCase)
            {
                if (state == null)
                {
                    state = new NFAState();
                }
                AddOut(new NFACharTransition(Char.ToLower(ch), state));
                AddOut(new NFACharTransition(Char.ToUpper(ch), state));
                return state;
            }
            else
            {
                if (state == null)
                {
                    state = FindUniqueCharTransition(ch);
                    if (state != null)
                    {
                        return state;
                    }
                    state = new NFAState();
                }
                return AddOut(new NFACharTransition(ch, state));
            }
        }

        public NFAState AddOut(NFATransition trans)
        {
            Array.Resize(ref Outgoing, Outgoing.Length + 1);
            Outgoing[Outgoing.Length - 1] = trans;
            if (trans is NFAEpsilonTransition)
            {
                EpsilonOut = true;
            }
            return trans.State;
        }

        public void MergeInto(NFAState state)
        {
            for (int i = 0; i < Incoming.Length; i++)
            {
                state.AddIn(Incoming[i]);
                Incoming[i].State = state;
            }
            Incoming = null;
            for (int i = 0; i < Outgoing.Length; i++)
            {
                state.AddOut(Outgoing[i]);
            }
            Outgoing = null;
        }

        private NFAState FindUniqueCharTransition(char ch)
        {
            NFATransition res = null;
            NFATransition trans;

            for (int i = 0; i < Outgoing.Length; i++)
            {
                trans = Outgoing[i];
                if (trans.Match(ch) && trans is NFACharTransition)
                {
                    if (res != null)
                    {
                        return null;
                    }
                    res = trans;
                }
            }
            for (int i = 0; res != null && i < Outgoing.Length; i++)
            {
                trans = Outgoing[i];
                if (trans != res && trans.State == res.State)
                {
                    return null;
                }
            }
            return res?.State;
        }

        public void MatchTransitions(char ch, NFAStateQueue queue, bool initial)
        {
            for (int i = 0; i < Outgoing.Length; i++)
            {
                var trans = Outgoing[i];
                var target = trans.State;
                if (initial && trans is NFAEpsilonTransition)
                {
                    target.MatchTransitions(ch, queue, true);
                }
                else if (trans.Match(ch))
                {
                    queue.AddLast(target);
                    if (target.EpsilonOut)
                    {
                        target.MatchEmpty(queue);
                    }
                }
            }
        }

        public void MatchEmpty(NFAStateQueue queue)
        {
            for (int i = 0; i < Outgoing.Length; i++)
            {
                var trans = Outgoing[i];
                if (trans is NFAEpsilonTransition)
                {
                    var target = trans.State;
                    queue.AddLast(target);
                    if (target.EpsilonOut)
                    {
                        target.MatchEmpty(queue);
                    }
                }
            }
        }
    }


    /**
     * An NFA state transition. A transition checks a single
     * character of input an determines if it is a match. If a match
     * is encountered, the NFA should move forward to the transition
     * state.
     */
    internal abstract class NFATransition
    {

        internal NFAState State;

        protected NFATransition(NFAState state)
        {
            this.State = state;
            this.State.AddIn(this);
        }

        public abstract bool IsAscii();

        public abstract bool Match(char ch);

        public abstract NFATransition Copy(NFAState state);
    }


    /**
     * The special epsilon transition. This transition matches the
     * empty input, i.e. it is an automatic transition that doesn't
     * read any input. As such, it returns false in the match method
     * and is handled specially everywhere.
     */
    internal class NFAEpsilonTransition : NFATransition
    {
        public NFAEpsilonTransition(NFAState state) : base(state)
        {
        }

        public override bool IsAscii()
        {
            return false;
        }

        public override bool Match(char ch)
        {
            return false;
        }

        public override NFATransition Copy(NFAState state)
        {
            return new NFAEpsilonTransition(state);
        }
    }


    /**
     * A single character match transition.
     */
    internal class NFACharTransition : NFATransition
    {
        private readonly char _match;

        public NFACharTransition(char match, NFAState state) : base(state)
        {
            _match = match;
        }

        public override bool IsAscii()
        {
            return 0 <= _match && _match < 128;
        }

        public override bool Match(char ch)
        {
            return this._match == ch;
        }

        public override NFATransition Copy(NFAState state)
        {
            return new NFACharTransition(_match, state);
        }
    }


    /**
     * A character range match transition. Used for user-defined
     * character sets in regular expressions.
     */
    internal class NFACharRangeTransition : NFATransition
    {

        protected bool Inverse;
        protected bool IgnoreCase;

        private object[] _contents = new object[0];

        public NFACharRangeTransition(bool inverse,
                                      bool ignoreCase,
                                      NFAState state) : base(state)
        {
            this.Inverse = inverse;
            this.IgnoreCase = ignoreCase;
        }

        public override bool IsAscii()
        {
            if (Inverse)
            {
                return false;
            }
            for (int i = 0; i < _contents.Length; i++)
            {
                var obj = _contents[i];
                if (obj is char)
                {
                    var c = (char)obj;
                    if (c < 0 || 128 <= c)
                    {
                        return false;
                    }
                }
                else if (obj is Range)
                {
                    if (!((Range)obj).IsAscii())
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        public void AddCharacter(char c)
        {
            if (IgnoreCase)
            {
                c = Char.ToLower(c);
            }
            AddContent(c);
        }

        public void AddRange(char min, char max)
        {
            if (IgnoreCase)
            {
                min = Char.ToLower(min);
                max = Char.ToLower(max);
            }
            AddContent(new Range(min, max));
        }

        private void AddContent(Object obj)
        {
            Array.Resize(ref _contents, _contents.Length + 1);
            _contents[_contents.Length - 1] = obj;
        }

        public override bool Match(char ch)
        {
            object obj;
            char c;
            Range r;

            if (IgnoreCase)
            {
                ch = Char.ToLower(ch);
            }
            for (int i = 0; i < _contents.Length; i++)
            {
                obj = _contents[i];
                if (obj is char)
                {
                    c = (char)obj;
                    if (c == ch)
                    {
                        return !Inverse;
                    }
                }
                else if (obj is Range)
                {
                    r = (Range)obj;
                    if (r.Inside(ch))
                    {
                        return !Inverse;
                    }
                }
            }
            return Inverse;
        }

        public override NFATransition Copy(NFAState state)
        {
            var copy = new NFACharRangeTransition(Inverse, IgnoreCase, state) { _contents = _contents };
            return copy;
        }

        private class Range
        {
            private readonly char _min;
            private readonly char _max;

            public Range(char min, char max)
            {
                this._min = min;
                this._max = max;
            }

            public bool IsAscii()
            {
                return 0 <= _min && _min < 128 &&
                       0 <= _max && _max < 128;
            }

            public bool Inside(char c)
            {
                return _min <= c && c <= _max;
            }
        }
    }


    /**
     * The dot ('.') character set transition. This transition
     * matches a single character that is not equal to a newline
     * character.
     */
    internal class NFADotTransition : NFATransition
    {
        public NFADotTransition(NFAState state) : base(state)
        {
        }

        public override bool IsAscii()
        {
            return false;
        }

        public override bool Match(char ch)
        {
            switch (ch)
            {
                case '\n':
                case '\r':
                case '\u0085':
                case '\u2028':
                case '\u2029':
                    return false;
                default:
                    return true;
            }
        }

        public override NFATransition Copy(NFAState state)
        {
            return new NFADotTransition(state);
        }
    }


    /**
     * The digit character set transition. This transition matches a
     * single numeric character.
     */
    internal class NFADigitTransition : NFATransition
    {
        public NFADigitTransition(NFAState state) : base(state)
        {
        }

        public override bool IsAscii()
        {
            return true;
        }

        public override bool Match(char ch)
        {
            return '0' <= ch && ch <= '9';
        }

        public override NFATransition Copy(NFAState state)
        {
            return new NFADigitTransition(state);
        }
    }


    /**
     * The non-digit character set transition. This transition
     * matches a single non-numeric character.
     */
    internal class NFANonDigitTransition : NFATransition
    {
        public NFANonDigitTransition(NFAState state) : base(state)
        {
        }

        public override bool IsAscii()
        {
            return false;
        }

        public override bool Match(char ch)
        {
            return ch < '0' || '9' < ch;
        }

        public override NFATransition Copy(NFAState state)
        {
            return new NFANonDigitTransition(state);
        }
    }

    /**
     * The whitespace character set transition. This transition
     * matches a single whitespace character.
     */
    internal class NFAWhitespaceTransition : NFATransition
    {
        public NFAWhitespaceTransition(NFAState state) : base(state)
        {
        }

        public override bool IsAscii()
        {
            return true;
        }

        public override bool Match(char ch)
        {
            switch (ch)
            {
                case ' ':
                case '\t':
                case '\n':
                case '\f':
                case '\r':
                case (char)11:
                    return true;
                default:
                    return false;
            }
        }

        public override NFATransition Copy(NFAState state)
        {
            return new NFAWhitespaceTransition(state);
        }
    }


    /**
     * The non-whitespace character set transition. This transition
     * matches a single non-whitespace character.
     */
    internal class NFANonWhitespaceTransition : NFATransition
    {

        public NFANonWhitespaceTransition(NFAState state) : base(state)
        {
        }

        public override bool IsAscii()
        {
            return false;
        }

        public override bool Match(char ch)
        {
            switch (ch)
            {
                case ' ':
                case '\t':
                case '\n':
                case '\f':
                case '\r':
                case (char)11:
                    return false;
                default:
                    return true;
            }
        }

        public override NFATransition Copy(NFAState state)
        {
            return new NFANonWhitespaceTransition(state);
        }
    }


    /**
     * The word character set transition. This transition matches a
     * single word character.
     */
    internal class NFAWordTransition : NFATransition
    {

        public NFAWordTransition(NFAState state) : base(state)
        {
        }

        public override bool IsAscii()
        {
            return true;
        }

        
        public override bool Match(char ch)
        {
            return ('a' <= ch && ch <= 'z')
                || ('A' <= ch && ch <= 'Z')
                || ('0' <= ch && ch <= '9')
                || ch == '_';
        }

        public override NFATransition Copy(NFAState state)
        {
            return new NFAWordTransition(state);
        }
    }


    /**
     * The non-word character set transition. This transition matches
     * a single non-word character.
     */
    internal class NFANonWordTransition : NFATransition
    {
        public NFANonWordTransition(NFAState state) : base(state)
        {
        }

        public override bool IsAscii()
        {
            return false;
        }

        public override bool Match(char ch)
        {
            bool word = ('a' <= ch && ch <= 'z')
                     || ('A' <= ch && ch <= 'Z')
                     || ('0' <= ch && ch <= '9')
                     || ch == '_';
            return !word;
        }

        public override NFATransition Copy(NFAState state)
        {
            return new NFANonWordTransition(state);
        }
    }


    /**
     * An NFA state queue. This queue is used during processing to
     * keep track of the current and subsequent NFA states. The
     * current state is read from the beginning of the queue, and new
     * states are added at the end. A marker index is used to
     * separate the current from the subsequent states.<p>
     *
     * The queue implementation is optimized for quick removal at the
     * beginning and addition at the end. It will attempt to use a
     * fixed-size array to store the whole queue, and moves the data
     * in this array only when absolutely needed. The array is also
     * enlarged automatically if too many states are being processed
     * at a single time.
     */
    internal class NFAStateQueue
    {

        private NFAState[] _queue = new NFAState[2048];

        private int _first = 0;

        private int _last = 0;

        private int _mark = 0;

        public bool Empty => (_last <= _first);

        public bool Marked => _first == _mark;

        public void Clear()
        {
            _first = 0;
            _last = 0;
            _mark = 0;
        }

        public void MarkEnd()
        {
            _mark = _last;
        }

        public NFAState RemoveFirst()
        {
            if (_first < _last)
            {
                _first++;
                return _queue[_first - 1];
            }
            else
            {
                return null;
            }
        }

        public void AddLast(NFAState state)
        {
            if (_last >= _queue.Length)
            {
                if (_first <= 0)
                {
                    Array.Resize(ref _queue, _queue.Length * 2);
                }
                else
                {
                    Array.Copy(_queue, _first, _queue, 0, _last - _first);
                    _last -= _first;
                    _mark -= _first;
                    _first = 0;
                }
            }
            _queue[_last++] = state;
        }
    }
}
