using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Flee.Parsing.grammatica_1._5.alpha2.PerCederberg.Grammatica.Runtime.RE;

namespace Flee.Parsing.grammatica_1._5.alpha2.PerCederberg.Grammatica.Runtime
{
    /**
     * A regular expression parser. The parser creates an NFA for the
     * regular expression having a single start and acceptance states.
   */
    internal class TokenRegExpParser
    {
        private readonly string _pattern;
        private readonly bool _ignoreCase;
        private int _pos;
        internal NFAState Start = new NFAState();
        internal NFAState End;
        private int _stateCount;
        private int _transitionCount;
        private int _epsilonCount;

        public TokenRegExpParser(string pattern) : this(pattern, false)
        {
        }

        public TokenRegExpParser(string pattern, bool ignoreCase)
        {
            this._pattern = pattern;
            this._ignoreCase = ignoreCase;
            this._pos = 0;
            this.End = ParseExpr(Start);
            if (_pos < pattern.Length)
            {
                throw new RegExpException(
                    RegExpException.ErrorType.UNEXPECTED_CHARACTER,
                    _pos,
                    pattern);
            }
        }

        public string GetDebugInfo()
        {
            if (_stateCount == 0)
            {
                UpdateStats(Start, new Hashtable());
            }
            return _stateCount + " states, " +
                   _transitionCount + " transitions, " +
                   _epsilonCount + " epsilons";
        }

        private void UpdateStats(NFAState state, Hashtable visited)
        {
            if (!visited.ContainsKey(state))
            {
                visited.Add(state, state);
                _stateCount++;
                for (int i = 0; i < state.Outgoing.Length; i++)
                {
                    _transitionCount++;
                    if (state.Outgoing[i] is NFAEpsilonTransition)
                    {
                        _epsilonCount++;
                    }
                    UpdateStats(state.Outgoing[i].State, visited);
                }
            }
        }

        private NFAState ParseExpr(NFAState start)
        {
            NFAState end = new NFAState();
            do
            {
                if (PeekChar(0) == '|')
                {
                    ReadChar('|');
                }
                var subStart = new NFAState();
                var subEnd = ParseTerm(subStart);
                if (subStart.Incoming.Length == 0)
                {
                    subStart.MergeInto(start);
                }
                else
                {
                    start.AddOut(new NFAEpsilonTransition(subStart));
                }
                if (subEnd.Outgoing.Length == 0 ||
                    (!end.HasTransitions() && PeekChar(0) != '|'))
                {
                    subEnd.MergeInto(end);
                }
                else
                {
                    subEnd.AddOut(new NFAEpsilonTransition(end));
                }
            } while (PeekChar(0) == '|');
            return end;
        }

        private NFAState ParseTerm(NFAState start)
        {
            var end = ParseFact(start);
            while (true)
            {
                switch (PeekChar(0))
                {
                    case -1:
                    case ')':
                    case ']':
                    case '{':
                    case '}':
                    case '?':
                    case '+':
                    case '|':
                        return end;
                    default:
                        end = ParseFact(end);
                        break;
                }
            }
        }

        private NFAState ParseFact(NFAState start)
        {
            NFAState placeholder = new NFAState();

            var end = ParseAtom(placeholder);
            switch (PeekChar(0))
            {
                case '?':
                case '*':
                case '+':
                case '{':
                    end = ParseAtomModifier(placeholder, end);
                    break;
            }
            if (placeholder.Incoming.Length > 0 && start.Outgoing.Length > 0)
            {
                start.AddOut(new NFAEpsilonTransition(placeholder));
                return end;
            }
            else
            {
                placeholder.MergeInto(start);
                return (end == placeholder) ? start : end;
            }
        }

        private NFAState ParseAtom(NFAState start)
        {
            NFAState end;

            switch (PeekChar(0))
            {
                case '.':
                    ReadChar('.');
                    return start.AddOut(new NFADotTransition(new NFAState()));
                case '(':
                    ReadChar('(');
                    end = ParseExpr(start);
                    ReadChar(')');
                    return end;
                case '[':
                    ReadChar('[');
                    end = ParseCharSet(start);
                    ReadChar(']');
                    return end;
                case -1:
                case ')':
                case ']':
                case '{':
                case '}':
                case '?':
                case '*':
                case '+':
                case '|':
                    throw new RegExpException(
                        RegExpException.ErrorType.UNEXPECTED_CHARACTER,
                        _pos,
                        _pattern);
                default:
                    return ParseChar(start);
            }
        }

        private NFAState ParseAtomModifier(NFAState start, NFAState end)
        {
            int min = 0;
            int max = -1;
            int firstPos = _pos;

            // Read min and max
            switch (ReadChar())
            {
                case '?':
                    min = 0;
                    max = 1;
                    break;
                case '*':
                    min = 0;
                    max = -1;
                    break;
                case '+':
                    min = 1;
                    max = -1;
                    break;
                case '{':
                    min = ReadNumber();
                    max = min;
                    if (PeekChar(0) == ',')
                    {
                        ReadChar(',');
                        max = -1;
                        if (PeekChar(0) != '}')
                        {
                            max = ReadNumber();
                        }
                    }
                    ReadChar('}');
                    if (max == 0 || (max > 0 && min > max))
                    {
                        throw new RegExpException(
                            RegExpException.ErrorType.INVALID_REPEAT_COUNT,
                            firstPos,
                            _pattern);
                    }
                    break;
                default:
                    throw new RegExpException(
                        RegExpException.ErrorType.UNEXPECTED_CHARACTER,
                        _pos - 1,
                        _pattern);
            }

            // Read possessive or reluctant modifiers
            if (PeekChar(0) == '?')
            {
                throw new RegExpException(
                    RegExpException.ErrorType.UNSUPPORTED_SPECIAL_CHARACTER,
                    _pos,
                    _pattern);
            }
            else if (PeekChar(0) == '+')
            {
                throw new RegExpException(
                    RegExpException.ErrorType.UNSUPPORTED_SPECIAL_CHARACTER,
                    _pos,
                    _pattern);
            }

            // Handle supported repeaters
            if (min == 0 && max == 1)
            {
                return start.AddOut(new NFAEpsilonTransition(end));
            }
            else if (min == 0 && max == -1)
            {
                if (end.Outgoing.Length == 0)
                {
                    end.MergeInto(start);
                }
                else
                {
                    end.AddOut(new NFAEpsilonTransition(start));
                }
                return start;
            }
            else if (min == 1 && max == -1)
            {
                if (start.Outgoing.Length == 1 &&
                    end.Outgoing.Length == 0 &&
                    end.Incoming.Length == 1 &&
                    start.Outgoing[0] == end.Incoming[0])
                {

                    end.AddOut(start.Outgoing[0].Copy(end));
                }
                else
                {
                    end.AddOut(new NFAEpsilonTransition(start));
                }
                return end;
            }
            else
            {
                throw new RegExpException(
                    RegExpException.ErrorType.INVALID_REPEAT_COUNT,
                    firstPos,
                    _pattern);
            }
        }

        private NFAState ParseCharSet(NFAState start)
        {
            NFAState end = new NFAState();
            NFACharRangeTransition range;

            if (PeekChar(0) == '^')
            {
                ReadChar('^');
                range = new NFACharRangeTransition(true, _ignoreCase, end);
            }
            else
            {
                range = new NFACharRangeTransition(false, _ignoreCase, end);
            }
            start.AddOut(range);
            while (PeekChar(0) > 0)
            {
                var min = (char)PeekChar(0);
                switch (min)
                {
                    case ']':
                        return end;
                    case '\\':
                        range.AddCharacter(ReadEscapeChar());
                        break;
                    default:
                        ReadChar(min);
                        if (PeekChar(0) == '-' &&
                            PeekChar(1) > 0 &&
                            PeekChar(1) != ']')
                        {

                            ReadChar('-');
                            var max = ReadChar();
                            range.AddRange(min, max);
                        }
                        else
                        {
                            range.AddCharacter(min);
                        }
                        break;
                }
            }
            return end;
        }

        private NFAState ParseChar(NFAState start)
        {
            switch (PeekChar(0))
            {
                case '\\':
                    return ParseEscapeChar(start);
                case '^':
                case '$':
                    throw new RegExpException(
                        RegExpException.ErrorType.UNSUPPORTED_SPECIAL_CHARACTER,
                        _pos,
                        _pattern);
                default:
                    return start.AddOut(ReadChar(), _ignoreCase, new NFAState());
            }
        }

        private NFAState ParseEscapeChar(NFAState start)
        {
            NFAState end = new NFAState();

            if (PeekChar(0) == '\\' && PeekChar(1) > 0)
            {
                switch ((char)PeekChar(1))
                {
                    case 'd':
                        ReadChar();
                        ReadChar();
                        return start.AddOut(new NFADigitTransition(end));
                    case 'D':
                        ReadChar();
                        ReadChar();
                        return start.AddOut(new NFANonDigitTransition(end));
                    case 's':
                        ReadChar();
                        ReadChar();
                        return start.AddOut(new NFAWhitespaceTransition(end));
                    case 'S':
                        ReadChar();
                        ReadChar();
                        return start.AddOut(new NFANonWhitespaceTransition(end));
                    case 'w':
                        ReadChar();
                        ReadChar();
                        return start.AddOut(new NFAWordTransition(end));
                    case 'W':
                        ReadChar();
                        ReadChar();
                        return start.AddOut(new NFANonWordTransition(end));
                }
            }
            return start.AddOut(ReadEscapeChar(), _ignoreCase, end);
        }

        private char ReadEscapeChar()
        {
            string str;
            int value;

            ReadChar('\\');
            var c = ReadChar();
            switch (c)
            {
                case '0':
                    c = ReadChar();
                    if (c < '0' || c > '3')
                    {
                        throw new RegExpException(
                            RegExpException.ErrorType.UNSUPPORTED_ESCAPE_CHARACTER,
                            _pos - 3,
                            _pattern);
                    }
                    value = c - '0';
                    c = (char)PeekChar(0);
                    if ('0' <= c && c <= '7')
                    {
                        value *= 8;
                        value += ReadChar() - '0';
                        c = (char)PeekChar(0);
                        if ('0' <= c && c <= '7')
                        {
                            value *= 8;
                            value += ReadChar() - '0';
                        }
                    }
                    return (char)value;
                case 'x':
                    str = ReadChar().ToString() + ReadChar().ToString();
                    try
                    {
                        value = Int32.Parse(str, NumberStyles.AllowHexSpecifier);
                        return (char)value;
                    }
                    catch (FormatException)
                    {
                        throw new RegExpException(
                            RegExpException.ErrorType.UNSUPPORTED_ESCAPE_CHARACTER,
                            _pos - str.Length - 2,
                            _pattern);
                    }
                case 'u':
                    str = ReadChar().ToString() +
                          ReadChar().ToString() +
                          ReadChar().ToString() +
                          ReadChar().ToString();
                    try
                    {
                        value = Int32.Parse(str, NumberStyles.AllowHexSpecifier);
                        return (char)value;
                    }
                    catch (FormatException)
                    {
                        throw new RegExpException(
                            RegExpException.ErrorType.UNSUPPORTED_ESCAPE_CHARACTER,
                            _pos - str.Length - 2,
                            _pattern);
                    }
                case 't':
                    return '\t';
                case 'n':
                    return '\n';
                case 'r':
                    return '\r';
                case 'f':
                    return '\f';
                case 'a':
                    return '\u0007';
                case 'e':
                    return '\u001B';
                default:
                    if (('A' <= c && c <= 'Z') || ('a' <= c && c <= 'z'))
                    {
                        throw new RegExpException(
                            RegExpException.ErrorType.UNSUPPORTED_ESCAPE_CHARACTER,
                            _pos - 2,
                            _pattern);
                    }
                    return c;
            }
        }

        private int ReadNumber()
        {
            StringBuilder buf = new StringBuilder();
            int c;

            c = PeekChar(0);
            while ('0' <= c && c <= '9')
            {
                buf.Append(ReadChar());
                c = PeekChar(0);
            }
            if (buf.Length <= 0)
            {
                throw new RegExpException(
                    RegExpException.ErrorType.UNEXPECTED_CHARACTER,
                    _pos,
                    _pattern);
            }
            return Int32.Parse(buf.ToString());
        }

        private char ReadChar()
        {
            int c = PeekChar(0);

            if (c < 0)
            {
                throw new RegExpException(
                    RegExpException.ErrorType.UNTERMINATED_PATTERN,
                    _pos,
                    _pattern);
            }
            else
            {
                _pos++;
                return (char)c;
            }
        }

        private char ReadChar(char c)
        {
            if (c != ReadChar())
            {
                throw new RegExpException(
                    RegExpException.ErrorType.UNEXPECTED_CHARACTER,
                    _pos - 1,
                    _pattern);
            }
            return c;
        }

        private int PeekChar(int count)
        {
            if (_pos + count < _pattern.Length)
            {
                return _pattern[_pos + count];
            }
            else
            {
                return -1;
            }
        }
    }
}
