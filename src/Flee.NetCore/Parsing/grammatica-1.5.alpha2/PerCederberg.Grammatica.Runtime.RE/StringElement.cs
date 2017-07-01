using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Diagnostics;


namespace Flee.Parsing.grammatica_1._5.alpha2.PerCederberg.Grammatica.Runtime.RE
{
    /**
     * A regular expression string element. This element only matches
     * an exact string. Once created, the string element is immutable.
     */
    internal class StringElement : Element
    {
        private readonly string _value;
        public StringElement(char c)
            : this(c.ToString())
        {
        }

        public StringElement(string str)
        {
            _value = str;
        }

        public string GetString()
        {
            return _value;
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
            if (skip != 0)
            {
                return -1;
            }
            for (int i = 0; i < _value.Length; i++)
            {
                var c = buffer.Peek(start + i);
                if (c < 0)
                {
                    m.SetReadEndOfString();
                    return -1;
                }
                if (m.IsCaseInsensitive())
                {
                    c = (int)Char.ToLower((char)c);
                }
                if (c != (int)_value[i])
                {
                    return -1;
                }
            }
            return _value.Length;
        }

        public override void PrintTo(TextWriter output, string indent)
        {
            output.WriteLine(indent + "'" + _value + "'");
        }
    }
}
