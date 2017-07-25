using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text;


namespace Flee.Parsing.grammatica_1._5.alpha2.PerCederberg.Grammatica.Runtime.RE
{
    /**
     * A regular expression. This class creates and holds an internal
     * data structure representing a regular expression. It also
     * allows creating matchers. This class is thread-safe. Multiple
     * matchers may operate simultanously on the same regular
     * expression.
     */
    internal class RegExp
    {
        private readonly Element _element;
        private readonly string _pattern;
        private readonly bool _ignoreCase;
        private int _pos;

        public RegExp(string pattern)
            : this(pattern, false)
        {
        }

        public RegExp(string pattern, bool ignoreCase)
        {
            this._pattern = pattern;
            this._ignoreCase = ignoreCase;
            this._pos = 0;
            this._element = ParseExpr();
            if (_pos < pattern.Length)
            {
                throw new RegExpException(
                    RegExpException.ErrorType.UNEXPECTED_CHARACTER,
                    _pos,
                    pattern);
            }
        }

        public Matcher Matcher(string str)
        {
            return Matcher(new ReaderBuffer(new StringReader(str)));
        }

        public Matcher Matcher(ReaderBuffer buffer)
        {
            return new Matcher((Element)_element.Clone(), buffer, _ignoreCase);
        }

        public override string ToString()
        {
            var str = new StringWriter();
            str.WriteLine("Regular Expression");
            str.WriteLine("  Pattern: " + _pattern);
            str.Write("  Flags:");
            if (_ignoreCase)
            {
                str.Write(" caseignore");
            }
            str.WriteLine();
            str.WriteLine("  Compiled:");
            _element.PrintTo(str, "    ");
            return str.ToString();
        }

        private Element ParseExpr()
        {
            var first = ParseTerm();
            if (PeekChar(0) != '|')
            {
                return first;
            }
            else
            {
                ReadChar('|');
                var second = ParseExpr();
                return new AlternativeElement(first, second);
            }
        }

        private Element ParseTerm()
        {
            ArrayList list = new ArrayList();

            list.Add(ParseFact());
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
                        return CombineElements(list);
                    default:
                        list.Add(ParseFact());
                        break;
                }
            }
        }

        private Element ParseFact()
        {
            var elem = ParseAtom();
            switch (PeekChar(0))
            {
                case '?':
                case '*':
                case '+':
                case '{':
                    return ParseAtomModifier(elem);
                default:
                    return elem;
            }
        }

        private Element ParseAtom()
        {
            Element elem;

            switch (PeekChar(0))
            {
                case '.':
                    ReadChar('.');
                    return CharacterSetElement.Dot;
                case '(':
                    ReadChar('(');
                    elem = ParseExpr();
                    ReadChar(')');
                    return elem;
                case '[':
                    ReadChar('[');
                    elem = ParseCharSet();
                    ReadChar(']');
                    return elem;
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
                    return ParseChar();
            }
        }

        private Element ParseAtomModifier(Element elem)
        {
            int min = 0;
            int max = -1;
            RepeatElement.RepeatType type;
            int firstPos;

            // Read min and max
            type = RepeatElement.RepeatType.GREEDY;
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
                    firstPos = _pos - 1;
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

            // Read operator mode
            if (PeekChar(0) == '?')
            {
                ReadChar('?');
                type = RepeatElement.RepeatType.RELUCTANT;
            }
            else if (PeekChar(0) == '+')
            {
                ReadChar('+');
                type = RepeatElement.RepeatType.POSSESSIVE;
            }

            return new RepeatElement(elem, min, max, type);
        }

        private Element ParseCharSet()
        {
            CharacterSetElement charset;
            bool repeat = true;

            if (PeekChar(0) == '^')
            {
                ReadChar('^');
                charset = new CharacterSetElement(true);
            }
            else
            {
                charset = new CharacterSetElement(false);
            }

            while (PeekChar(0) > 0 && repeat)
            {
                var start = (char)PeekChar(0);
                switch (start)
                {
                    case ']':
                        repeat = false;
                        break;
                    case '\\':
                        var elem = ParseEscapeChar();
                        if (elem is StringElement)
                        {
                            charset.AddCharacters((StringElement)elem);
                        }
                        else
                        {
                            charset.AddCharacterSet((CharacterSetElement)elem);
                        }
                        break;
                    default:
                        ReadChar(start);
                        if (PeekChar(0) == '-'
                            && PeekChar(1) > 0
                            && PeekChar(1) != ']')
                        {

                            ReadChar('-');
                            var end = ReadChar();
                            charset.AddRange(FixChar(start), FixChar(end));
                        }
                        else
                        {
                            charset.AddCharacter(FixChar(start));
                        }
                        break;
                }
            }

            return charset;
        }

        private Element ParseChar()
        {
            switch (PeekChar(0))
            {
                case '\\':
                    return ParseEscapeChar();
                case '^':
                case '$':
                    throw new RegExpException(
                        RegExpException.ErrorType.UNSUPPORTED_SPECIAL_CHARACTER,
                        _pos,
                        _pattern);
                default:
                    return new StringElement(FixChar(ReadChar()));
            }
        }

        private Element ParseEscapeChar()
        {
            char c;
            string str;
            int value;

            ReadChar('\\');
            c = ReadChar();
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
                    return new StringElement(FixChar((char)value));
                case 'x':
                    str = ReadChar().ToString() +
                          ReadChar().ToString();
                    try
                    {
                        value = Int32.Parse(str,
                                            NumberStyles.AllowHexSpecifier);
                        return new StringElement(FixChar((char)value));
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
                        value = Int32.Parse(str,
                                            NumberStyles.AllowHexSpecifier);
                        return new StringElement(FixChar((char)value));
                    }
                    catch (FormatException)
                    {
                        throw new RegExpException(
                            RegExpException.ErrorType.UNSUPPORTED_ESCAPE_CHARACTER,
                            _pos - str.Length - 2,
                            _pattern);
                    }
                case 't':
                    return new StringElement('\t');
                case 'n':
                    return new StringElement('\n');
                case 'r':
                    return new StringElement('\r');
                case 'f':
                    return new StringElement('\f');
                case 'a':
                    return new StringElement('\u0007');
                case 'e':
                    return new StringElement('\u001B');
                case 'd':
                    return CharacterSetElement.Digit;
                case 'D':
                    return CharacterSetElement.NonDigit;
                case 's':
                    return CharacterSetElement.Whitespace;
                case 'S':
                    return CharacterSetElement.NonWhitespace;
                case 'w':
                    return CharacterSetElement.Word;
                case 'W':
                    return CharacterSetElement.NonWord;
                default:
                    if (('A' <= c && c <= 'Z') || ('a' <= c && c <= 'z'))
                    {
                        throw new RegExpException(
                            RegExpException.ErrorType.UNSUPPORTED_ESCAPE_CHARACTER,
                            _pos - 2,
                            _pattern);
                    }
                    return new StringElement(FixChar(c));
            }
        }

        private char FixChar(char c)
        {
            return _ignoreCase ? Char.ToLower(c) : c;
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

        private Element CombineElements(ArrayList list)
        {
            Element elem;
            int i;
            // Concatenate string elements
            var prev = (Element)list[0];
            for (i = 1; i < list.Count; i++)
            {
                elem = (Element)list[i];
                if (prev is StringElement
                 && elem is StringElement)
                {

                    var str = ((StringElement)prev).GetString() +
                                 ((StringElement)elem).GetString();
                    elem = new StringElement(str);
                    list.RemoveAt(i);
                    list[i - 1] = elem;
                    i--;
                }
                prev = elem;
            }

            // Combine all remaining elements
            elem = (Element)list[list.Count - 1];
            for (i = list.Count - 2; i >= 0; i--)
            {
                prev = (Element)list[i];
                elem = new CombineElement(prev, elem);
            }

            return elem;
        }
    }
}
