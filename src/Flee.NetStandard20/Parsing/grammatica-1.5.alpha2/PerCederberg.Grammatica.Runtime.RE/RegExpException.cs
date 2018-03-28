using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;


namespace Flee.Parsing.grammatica_1._5.alpha2.PerCederberg.Grammatica.Runtime.RE
{
    /**
     * A regular expression exception. This exception is thrown if a
     * regular expression couldn't be processed (or "compiled")
     * properly.
     */
    internal class RegExpException : Exception
    {
        public enum ErrorType
        {

            /**
             * The unexpected character error constant. This error is
             * used when a character was read that didn't match the
             * allowed set of characters at the given position.
             */
            UNEXPECTED_CHARACTER,

            /**
             * The unterminated pattern error constant. This error is
             * used when more characters were expected in the pattern.
             */
            UNTERMINATED_PATTERN,

            /**
             * The unsupported special character error constant. This
             * error is used when special regular expression
             * characters are used in the pattern, but not supported
             * in this implementation.
             */
            UNSUPPORTED_SPECIAL_CHARACTER,

            /**
             * The unsupported escape character error constant. This
             * error is used when an escape character construct is
             * used in the pattern, but not supported in this
             * implementation.
             */
            UNSUPPORTED_ESCAPE_CHARACTER,

            /**
             * The invalid repeat count error constant. This error is
             * used when a repetition count of zero is specified, or
             * when the minimum exceeds the maximum.
             */
            INVALID_REPEAT_COUNT
        }

        private readonly ErrorType _type;
        private readonly int _position;
        private readonly string _pattern;

        public RegExpException(ErrorType type, int pos, string pattern)
        {
            this._type = type;
            this._position = pos;
            this._pattern = pattern;
        }

        public override string Message => GetMessage();

        public string GetMessage()
        {
            StringBuilder buffer = new StringBuilder();

            // Append error type name
            switch (_type)
            {
                case ErrorType.UNEXPECTED_CHARACTER:
                    buffer.Append("unexpected character");
                    break;
                case ErrorType.UNTERMINATED_PATTERN:
                    buffer.Append("unterminated pattern");
                    break;
                case ErrorType.UNSUPPORTED_SPECIAL_CHARACTER:
                    buffer.Append("unsupported character");
                    break;
                case ErrorType.UNSUPPORTED_ESCAPE_CHARACTER:
                    buffer.Append("unsupported escape character");
                    break;
                case ErrorType.INVALID_REPEAT_COUNT:
                    buffer.Append("invalid repeat count");
                    break;
                default:
                    buffer.Append("internal error");
                    break;
            }

            // Append erroneous character
            buffer.Append(": ");
            if (_position < _pattern.Length)
            {
                buffer.Append('\'');
                buffer.Append(_pattern.Substring(_position));
                buffer.Append('\'');
            }
            else
            {
                buffer.Append("<end of pattern>");
            }

            // Append position
            buffer.Append(" at position ");
            buffer.Append(_position);

            return buffer.ToString();
        }
    }
}
