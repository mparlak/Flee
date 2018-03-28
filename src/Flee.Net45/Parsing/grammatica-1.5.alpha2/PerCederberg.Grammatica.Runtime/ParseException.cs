using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Flee.Resources;

namespace Flee.Parsing.grammatica_1._5.alpha2.PerCederberg.Grammatica.Runtime
{
    /**
     * A parse exception.
     */
    public class ParseException : Exception
    {
        public enum ErrorType
        {

            /**
             * The internal error type is only used to signal an error
             * that is a result of a bug in the parser or tokenizer
             * code.
             */
            INTERNAL,

            /**
             * The I/O error type is used for stream I/O errors.
             */
            IO,

            /**
             * The unexpected end of file error type is used when end
             * of file is encountered instead of a valid token.
             */
            UNEXPECTED_EOF,

            /**
             * The unexpected character error type is used when a
             * character is read that isn't handled by one of the
             * token patterns.
             */
            UNEXPECTED_CHAR,

            /**
             * The unexpected token error type is used when another
             * token than the expected one is encountered.
             */
            UNEXPECTED_TOKEN,

            /**
             * The invalid token error type is used when a token
             * pattern with an error message is matched. The
             * additional information provided should contain the
             * error message.
             */
            INVALID_TOKEN,

            /**
             * The analysis error type is used when an error is
             * encountered in the analysis. The additional information
             * provided should contain the error message.
             */
            ANALYSIS
        }

        private readonly ErrorType _type;
        private readonly string _info;
        private readonly ArrayList _details;
        private readonly int _line;
        private readonly int _column;


        /// <summary>
        /// Creates a new parse exception.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="info"></param>
        /// <param name="line"></param>
        /// <param name="column"></param>
        public ParseException(ErrorType type,
                              string info,
                              int line,
                              int column)
            : this(type, info, null, line, column)
        {
        }

        /// <summary>
        /// Creates a new parse exception. This constructor is only
        /// used to supply the detailed information array, which is
        /// only used for expected token errors. The list then contains
        /// descriptions of the expected tokens.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="info"></param>
        /// <param name="details"></param>
        /// <param name="line"></param>
        /// <param name="column"></param>
        public ParseException(ErrorType type,
                              string info,
                              ArrayList details,
                              int line,
                              int column)
        {

            this._type = type;
            this._info = info;
            this._details = details;
            this._line = line;
            this._column = column;
        }

        
        public ErrorType Type => _type;

        public ErrorType GetErrorType()
        {
            return Type;
        }

        public string Info => _info;

        public string GetInfo()
        {
            return Info;
        }

        public ArrayList Details => new ArrayList(_details);

        public ArrayList GetDetails()
        {
            return Details;
        }

        public int Line => _line;

        public int GetLine()
        {
            return Line;
        }

        public int Column => _column;

        public int GetColumn()
        {
            return _column;
        }

        public override string Message
        {
            get
            {
                StringBuilder buffer = new StringBuilder();

                // Add error description
                buffer.Append(ErrorMessage);

                // Add line and column
                if (_line > 0 && _column > 0)
                {
                    buffer.Append(", on line: ");
                    buffer.Append(_line);
                    buffer.Append(" column: ");
                    buffer.Append(_column);
                }

                return buffer.ToString();
            }
        }

        public string GetMessage()
        {
            return Message;
        }

        public string ErrorMessage
        {
            get
            {
                StringBuilder buffer = new StringBuilder();

                // Add type and info
                switch (_type)
                {
                    case ErrorType.IO:
                        buffer.Append("I/O error: ");
                        buffer.Append(_info);
                        break;
                    case ErrorType.UNEXPECTED_EOF:
                        buffer.Append("unexpected end of file");
                        break;
                    case ErrorType.UNEXPECTED_CHAR:
                        buffer.Append("unexpected character '");
                        buffer.Append(_info);
                        buffer.Append("'");
                        break;
                    case ErrorType.UNEXPECTED_TOKEN:
                        buffer.Append("unexpected token ");
                        buffer.Append(_info);
                        if (_details != null)
                        {
                            buffer.Append(", expected ");
                            if (_details.Count > 1)
                            {
                                buffer.Append("one of ");
                            }
                            buffer.Append(GetMessageDetails());
                        }
                        break;
                    case ErrorType.INVALID_TOKEN:
                        buffer.Append(_info);
                        break;
                    case ErrorType.ANALYSIS:
                        buffer.Append(_info);
                        break;
                    default:
                        buffer.Append("internal error");
                        if (_info != null)
                        {
                            buffer.Append(": ");
                            buffer.Append(_info);
                        }
                        break;
                }

                return buffer.ToString();
            }
        }

        public string GetErrorMessage()
        {
            return ErrorMessage;
        }

        private string GetMessageDetails()
        {
            StringBuilder buffer = new StringBuilder();

            for (int i = 0; i < _details.Count; i++)
            {
                if (i > 0)
                {
                    buffer.Append(", ");
                    if (i + 1 == _details.Count)
                    {
                        buffer.Append("or ");
                    }
                }
                buffer.Append(_details[i]);
            }

            return buffer.ToString();
        }
    }
}
