using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Flee.Parsing.grammatica_1._5.alpha2.PerCederberg.Grammatica.Runtime
{
    /**
     * A token pattern. This class contains the definition of a token
     * (i.e. it's pattern), and allows testing a string against this
     * pattern. A token pattern is uniquely identified by an integer id,
     * that must be provided upon creation.
     *
    
     */
    internal class TokenPattern
    {
        public enum PatternType
        {

            /**
             * The string pattern type is used for tokens that only
             * match an exact string.
             */
            STRING,

            /**
             * The regular expression pattern type is used for tokens
             * that match a regular expression.
             */
            REGEXP
        }

        private int _id;
        private string _name;
        private PatternType _type;
        private string _pattern;
        private bool _error;
        private string _errorMessage;
        private bool _ignore;
        private string _ignoreMessage;
        private string _debugInfo;

        public TokenPattern(int id,
                            string name,
                            PatternType type,
                            string pattern)
        {

            this._id = id;
            this._name = name;
            this._type = type;
            this._pattern = pattern;
        }

        public int Id
        {
            get
            {
                return _id;
            }
            set { _id = value; }
        }

        public int GetId()
        {
            return _id;
        }

        public string Name
        {
            get
            {
                return _name;
            }
            set { _name = value; }
        }

        public string GetName()
        {
            return _name;
        }

        public PatternType Type
        {
            get
            {
                return _type;
            }
            set { _type = value; }
        }

        public PatternType GetPatternType()
        {
            return _type;
        }

        public string Pattern
        {
            get
            {
                return _pattern;
            }
            set { _pattern = value; }
        }

        public string GetPattern()
        {
            return _pattern;
        }

        public bool Error
        {
            get
            {
                return _error;
            }
            set
            {
                _error = value;
                if (_error && _errorMessage == null)
                {
                    _errorMessage = "unrecognized token found";
                }
            }
        }

        public string ErrorMessage
        {
            get
            {
                return _errorMessage;
            }
            set
            {
                _error = true;
                _errorMessage = value;
            }
        }

        public bool IsError()
        {
            return Error;
        }

        public string GetErrorMessage()
        {
            return ErrorMessage;
        }

        public void SetError()
        {
            Error = true;
        }

        public void SetError(string message)
        {
            ErrorMessage = message;
        }

        public bool Ignore
        {
            get
            {
                return _ignore;
            }
            set
            {
                _ignore = value;
            }
        }

        public string IgnoreMessage
        {
            get
            {
                return _ignoreMessage;
            }
            set
            {
                _ignore = true;
                _ignoreMessage = value;
            }
        }

        public bool IsIgnore()
        {
            return Ignore;
        }

        public string GetIgnoreMessage()
        {
            return IgnoreMessage;
        }

       
        public void SetIgnore()
        {
            Ignore = true;
        }

        
        public void SetIgnore(string message)
        {
            IgnoreMessage = message;
        }

        public string DebugInfo
        {
            get
            {
                return _debugInfo;
            }
            set
            {
                _debugInfo = value;
            }
        }

        public override string ToString()
        {
            StringBuilder buffer = new StringBuilder();

            buffer.Append(_name);
            buffer.Append(" (");
            buffer.Append(_id);
            buffer.Append("): ");
            switch (_type)
            {
                case PatternType.STRING:
                    buffer.Append("\"");
                    buffer.Append(_pattern);
                    buffer.Append("\"");
                    break;
                case PatternType.REGEXP:
                    buffer.Append("<<");
                    buffer.Append(_pattern);
                    buffer.Append(">>");
                    break;
            }
            if (_error)
            {
                buffer.Append(" ERROR: \"");
                buffer.Append(_errorMessage);
                buffer.Append("\"");
            }
            if (_ignore)
            {
                buffer.Append(" IGNORE");
                if (_ignoreMessage != null)
                {
                    buffer.Append(": \"");
                    buffer.Append(_ignoreMessage);
                    buffer.Append("\"");
                }
            }
            if (_debugInfo != null)
            {
                buffer.Append("\n  ");
                buffer.Append(_debugInfo);
            }
            return buffer.ToString();
        }

        public string ToShortString()
        {
            StringBuilder buffer = new StringBuilder();
            int newline = _pattern.IndexOf('\n');

            if (_type == PatternType.STRING)
            {
                buffer.Append("\"");
                if (newline >= 0)
                {
                    if (newline > 0 && _pattern[newline - 1] == '\r')
                    {
                        newline--;
                    }
                    buffer.Append(_pattern.Substring(0, newline));
                    buffer.Append("(...)");
                }
                else
                {
                    buffer.Append(_pattern);
                }
                buffer.Append("\"");
            }
            else
            {
                buffer.Append("<");
                buffer.Append(_name);
                buffer.Append(">");
            }

            return buffer.ToString();
        }

        public void SetData(int id, string name, PatternType type, string pattern)
        {
            Id = id;
            Name = name;
            Type = type;
            Pattern = pattern;
        }
    }
}
