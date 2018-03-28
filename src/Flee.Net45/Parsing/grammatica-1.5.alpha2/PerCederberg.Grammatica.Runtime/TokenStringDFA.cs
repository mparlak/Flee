using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flee.Parsing.grammatica_1._5.alpha2.PerCederberg.Grammatica.Runtime
{
    /**
     * A deterministic finite state automaton for matching exact strings.
     * It uses a sorted binary tree representation of the state
     * transitions in order to enable quick matches with a minimal memory
     * footprint. It only supports a single character transition between
     * states, but may be run in an all case-insensitive mode.
     */
    internal class TokenStringDFA
    {

        private readonly DFAState[] _ascii = new DFAState[128];
        private readonly DFAState _nonAscii = new DFAState();

        public TokenStringDFA()
        {
        }

        public void AddMatch(string str, bool caseInsensitive, TokenPattern value)
        {
            DFAState state;
            char c = str[0];
            int start = 0;

            if (caseInsensitive)
            {
                c = Char.ToLower(c);
            }
            if (c < 128)
            {
                state = _ascii[c];
                if (state == null)
                {
                    state = _ascii[c] = new DFAState();
                }
                start++;
            }
            else
            {
                state = _nonAscii;
            }
            for (int i = start; i < str.Length; i++)
            {
                var next = state.Tree.Find(str[i], caseInsensitive);
                if (next == null)
                {
                    next = new DFAState();
                    state.Tree.Add(str[i], caseInsensitive, next);
                }
                state = next;
            }
            state.Value = value;
        }

        public TokenPattern Match(ReaderBuffer buffer, bool caseInsensitive)
        {
            TokenPattern result = null;
            DFAState state;
            int pos = 0;

            var c = buffer.Peek(0);
            if (c < 0)
            {
                return null;
            }
            if (caseInsensitive)
            {
                c = Char.ToLower((char)c);
            }
            if (c < 128)
            {
                state = _ascii[c];
                if (state == null)
                {
                    return null;
                }
                else if (state.Value != null)
                {
                    result = state.Value;
                }
                pos++;
            }
            else
            {
                state = _nonAscii;
            }
            while ((c = buffer.Peek(pos)) >= 0)
            {
                state = state.Tree.Find((char)c, caseInsensitive);
                if (state == null)
                {
                    break;
                }
                else if (state.Value != null)
                {
                    result = state.Value;
                }
                pos++;
            }
            return result;
        }

        public override string ToString()
        {
            StringBuilder buffer = new StringBuilder();

            for (int i = 0; i < _ascii.Length; i++)
            {
                if (_ascii[i] != null)
                {
                    buffer.Append((char)i);
                    if (_ascii[i].Value != null)
                    {
                        buffer.Append(": ");
                        buffer.Append(_ascii[i].Value);
                        buffer.Append("\n");
                    }
                    _ascii[i].Tree.PrintTo(buffer, " ");
                }
            }
            _nonAscii.Tree.PrintTo(buffer, "");
            return buffer.ToString();
        }
    }

    internal class DFAState
    {

        internal TokenPattern Value;

        internal TransitionTree Tree = new TransitionTree();
    }


    internal class TransitionTree
    {
        private char _value = '\0';
        private DFAState _state;
        private TransitionTree _left;
        private TransitionTree _right;

        public TransitionTree()
        {
        }

        public DFAState Find(char c, bool lowerCase)
        {
            if (lowerCase)
            {
                c = Char.ToLower(c);
            }
            if (_value == '\0' || _value == c)
            {
                return _state;
            }
            else if (_value > c)
            {
                return _left.Find(c, false);
            }
            else
            {
                return _right.Find(c, false);
            }
        }

        public void Add(char c, bool lowerCase, DFAState state)
        {
            if (lowerCase)
            {
                c = Char.ToLower(c);
            }
            if (_value == '\0')
            {
                this._value = c;
                this._state = state;
                this._left = new TransitionTree();
                this._right = new TransitionTree();
            }
            else if (_value > c)
            {
                _left.Add(c, false, state);
            }
            else
            {
                _right.Add(c, false, state);
            }
        }

        public void PrintTo(StringBuilder buffer, String indent)
        {
            _left?.PrintTo(buffer, indent);
            if (this._value != '\0')
            {
                if (buffer.Length > 0 && buffer[buffer.Length - 1] == '\n')
                {
                    buffer.Append(indent);
                }
                buffer.Append(this._value);
                if (this._state.Value != null)
                {
                    buffer.Append(": ");
                    buffer.Append(this._state.Value);
                    buffer.Append("\n");
                }
                this._state.Tree.PrintTo(buffer, indent + " ");
            }
            _right?.PrintTo(buffer, indent);
        }
    }
}
