using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;


namespace Flee.Parsing.grammatica_1._5.alpha2.PerCederberg.Grammatica.Runtime.RE
{
    /**
     * A regular expression string matcher. This class handles the
     * matching of a specific string with a specific regular
     * expression. It contains state information about the matching
     * process, as for example the position of the latest match, and a
     * number of flags that were set. This class is not thread-safe.
     */
    internal class Matcher
    {
        private readonly Element _element;
        private ReaderBuffer _buffer;
        private readonly bool _ignoreCase;
        private int _start;
        private int _length;
        private bool _endOfString;

        internal Matcher(Element e, ReaderBuffer buffer, bool ignoreCase)
        {
            this._element = e;
            this._buffer = buffer;
            this._ignoreCase = ignoreCase;
            this._start = 0;
            Reset();
        }

        public bool IsCaseInsensitive()
        {
            return _ignoreCase;
        }

        public void Reset()
        {
            _length = -1;
            _endOfString = false;
        }

        public void Reset(string str)
        {
            Reset(new ReaderBuffer(new StringReader(str)));
        }

        public void Reset(ReaderBuffer buffer)
        {
            this._buffer = buffer;
            Reset();
        }

        public int Start()
        {
            return _start;
        }

        public int End()
        {
            if (_length > 0)
            {
                return _start + _length;
            }
            else
            {
                return _start;
            }
        }

        public int Length()
        {
            return _length;
        }

        public bool HasReadEndOfString()
        {
            return _endOfString;
        }

        public bool MatchFromBeginning()
        {
            return MatchFrom(0);
        }

        public bool MatchFrom(int pos)
        {
            Reset();
            _start = pos;
            _length = _element.Match(this, _buffer, _start, 0);
            return _length >= 0;
        }

        public override string ToString()
        {
            if (_length <= 0)
            {
                return "";
            }
            else
            {
                return _buffer.Substring(_buffer.Position, _length);
            }
        }

        internal void SetReadEndOfString()
        {
            _endOfString = true;
        }
    }
}
