using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

namespace Flee.Parsing.grammatica_1._5.alpha2.PerCederberg.Grammatica.Runtime
{
    /**
     * A token node. This class represents a token (i.e. a set of adjacent
     * characters) in a parse tree. The tokens are created by a tokenizer,
     * that groups characters together into tokens according to a set of
     * token patterns.
     */
    internal class Token : Node
    {
        private readonly TokenPattern _pattern;
        private readonly string _image;
        private readonly int _startLine;
        private readonly int _startColumn;
        private readonly int _endLine;
        private readonly int _endColumn;
        private Token _previous = null;
        private Token _next = null;

        public Token(TokenPattern pattern, string image, int line, int col)
        {
            this._pattern = pattern;
            this._image = image;
            this._startLine = line;
            this._startColumn = col;
            this._endLine = line;
            this._endColumn = col + image.Length - 1;
            for (int pos = 0; image.IndexOf('\n', pos) >= 0;)
            {
                pos = image.IndexOf('\n', pos) + 1;
                this._endLine++;
                _endColumn = image.Length - pos;
            }
        }

        public override int Id => _pattern.Id;

        public override string Name => _pattern.Name;

        public override int StartLine => _startLine;

        public override int StartColumn => _startColumn;

        public override int EndLine => _endLine;

        public override int EndColumn => _endColumn;

        public string Image => _image;

        public string GetImage()
        {
            return Image;
        }

        internal TokenPattern Pattern => _pattern;
        public Token Previous
        {
            get
            {
                return _previous;
            }
            set
            {
                if (_previous != null)
                {
                    _previous._next = null;
                }
                _previous = value;
                if (_previous != null)
                {
                    _previous._next = this;
                }
            }
        }

        public Token GetPreviousToken()
        {
            return Previous;
        }

        public Token Next
        {
            get
            {
                return _next;
            }
            set
            {
                if (_next != null)
                {
                    _next._previous = null;
                }
                _next = value;
                if (_next != null)
                {
                    _next._previous = this;
                }
            }
        }

        public Token GetNextToken()
        {
            return Next;
        }

        public override string ToString()
        {
            StringBuilder buffer = new StringBuilder();
            int newline = _image.IndexOf('\n');

            buffer.Append(_pattern.Name);
            buffer.Append("(");
            buffer.Append(_pattern.Id);
            buffer.Append("): \"");
            if (newline >= 0)
            {
                if (newline > 0 && _image[newline - 1] == '\r')
                {
                    newline--;
                }
                buffer.Append(_image.Substring(0, newline));
                buffer.Append("(...)");
            }
            else
            {
                buffer.Append(_image);
            }
            buffer.Append("\", line: ");
            buffer.Append(_startLine);
            buffer.Append(", col: ");
            buffer.Append(_startColumn);

            return buffer.ToString();
        }

        public string ToShortString()
        {
            StringBuilder buffer = new StringBuilder();
            int newline = _image.IndexOf('\n');

            buffer.Append('"');
            if (newline >= 0)
            {
                if (newline > 0 && _image[newline - 1] == '\r')
                {
                    newline--;
                }
                buffer.Append(_image.Substring(0, newline));
                buffer.Append("(...)");
            }
            else
            {
                buffer.Append(_image);
            }
            buffer.Append('"');
            if (_pattern.Type == TokenPattern.PatternType.REGEXP)
            {
                buffer.Append(" <");
                buffer.Append(_pattern.Name);
                buffer.Append(">");
            }

            return buffer.ToString();
        }
    }
}
