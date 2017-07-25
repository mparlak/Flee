using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Flee.Parsing.grammatica_1._5.alpha2.PerCederberg.Grammatica.Runtime.RE
{
    /**
     * A regular expression character set element. This element
     * matches a single character inside (or outside) a character set.
     * The character set is user defined and may contain ranges of
     * characters. The set may also be inverted, meaning that only
     * characters not inside the set will be considered to match.
     */
    internal class CharacterSetElement : Element
    {
        public static CharacterSetElement Dot = new CharacterSetElement(false);
        public static CharacterSetElement Digit = new CharacterSetElement(false);
        public static CharacterSetElement NonDigit = new CharacterSetElement(true);
        public static CharacterSetElement Whitespace = new CharacterSetElement(false);
        public static CharacterSetElement NonWhitespace = new CharacterSetElement(true);
        public static CharacterSetElement Word = new CharacterSetElement(false);
        public static CharacterSetElement NonWord = new CharacterSetElement(true);
        private readonly bool _inverted;
        private readonly ArrayList _contents = new ArrayList();

        public CharacterSetElement(bool inverted)
        {
            this._inverted = inverted;
        }

        public void AddCharacter(char c)
        {
            _contents.Add(c);
        }

        public void AddCharacters(string str)
        {
            for (int i = 0; i < str.Length; i++)
            {
                AddCharacter(str[i]);
            }
        }

        public void AddCharacters(StringElement elem)
        {
            AddCharacters(elem.GetString());
        }

        public void AddRange(char min, char max)
        {
            _contents.Add(new Range(min, max));
        }

        public void AddCharacterSet(CharacterSetElement elem)
        {
            _contents.Add(elem);
        }

        public override object Clone()
        {
            return this;
        }

        public override int Match(Matcher m,
                                  ReaderBuffer buffer,
                                  int start,
                                  int skip)
        {

            int c;

            if (skip != 0)
            {
                return -1;
            }
            c = buffer.Peek(start);
            if (c < 0)
            {
                m.SetReadEndOfString();
                return -1;
            }
            if (m.IsCaseInsensitive())
            {
                c = (int)Char.ToLower((char)c);
            }
            return InSet((char)c) ? 1 : -1;
        }

        private bool InSet(char c)
        {
            if (this == Dot)
            {
                return InDotSet(c);
            }
            else if (this == Digit || this == NonDigit)
            {
                return InDigitSet(c) != _inverted;
            }
            else if (this == Whitespace || this == NonWhitespace)
            {
                return InWhitespaceSet(c) != _inverted;
            }
            else if (this == Word || this == NonWord)
            {
                return InWordSet(c) != _inverted;
            }
            else
            {
                return InUserSet(c) != _inverted;
            }
        }

        private bool InDotSet(char c)
        {
            switch (c)
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

        private bool InDigitSet(char c)
        {
            return '0' <= c && c <= '9';
        }

        private bool InWhitespaceSet(char c)
        {
            switch (c)
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

        private bool InWordSet(char c)
        {
            return ('a' <= c && c <= 'z')
                || ('A' <= c && c <= 'Z')
                || ('0' <= c && c <= '9')
                || c == '_';
        }

        private bool InUserSet(char value)
        {
            for (int i = 0; i < _contents.Count; i++)
            {
                var obj = _contents[i];
                if (obj is char)
                {
                    var c = (char)obj;
                    if (c == value)
                    {
                        return true;
                    }
                }
                else if (obj is Range)
                {
                    var r = (Range)obj;
                    if (r.Inside(value))
                    {
                        return true;
                    }
                }
                else if (obj is CharacterSetElement)
                {
                    var e = (CharacterSetElement)obj;
                    if (e.InSet(value))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public override void PrintTo(TextWriter output, string indent)
        {
            output.WriteLine(indent + ToString());
        }

        public override string ToString()
        {
            // Handle predefined character sets
            if (this == Dot)
            {
                return ".";
            }
            else if (this == Digit)
            {
                return "\\d";
            }
            else if (this == NonDigit)
            {
                return "\\D";
            }
            else if (this == Whitespace)
            {
                return "\\s";
            }
            else if (this == NonWhitespace)
            {
                return "\\S";
            }
            else if (this == Word)
            {
                return "\\w";
            }
            else if (this == NonWord)
            {
                return "\\W";
            }

            // Handle user-defined character sets
            var buffer = new StringBuilder();
            if (_inverted)
            {
                buffer.Append("^[");
            }
            else
            {
                buffer.Append("[");
            }
            for (int i = 0; i < _contents.Count; i++)
            {
                buffer.Append(_contents[i]);
            }
            buffer.Append("]");

            return buffer.ToString();
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

            public bool Inside(char c)
            {
                return _min <= c && c <= _max;
            }

            public override string ToString()
            {
                return _min + "-" + _max;
            }
        }
    }
}
