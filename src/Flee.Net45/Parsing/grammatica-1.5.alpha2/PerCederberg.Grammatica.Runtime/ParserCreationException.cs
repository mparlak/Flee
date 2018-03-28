using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Flee.Parsing.grammatica_1._5.alpha2.PerCederberg.Grammatica.Runtime
{
    /**
     * A parser creation exception. This exception is used for signalling
     * an error in the token or production patterns, making it impossible
     * to create a working parser or tokenizer.
     */
    internal class ParserCreationException : Exception
    {

        /**
         * The error type enumeration.
         */
        public enum ErrorType
        {

            /**
             * The internal error type is only used to signal an
             * error that is a result of a bug in the parser or
             * tokenizer code.
             */
            INTERNAL,

            /**
             * The invalid parser error type is used when the parser
             * as such is invalid. This error is typically caused by
             * using a parser without any patterns.
             */
            INVALID_PARSER,

            /**
             * The invalid token error type is used when a token
             * pattern is erroneous. This error is typically caused
             * by an invalid pattern type or an erroneous regular
             * expression.
             */
            INVALID_TOKEN,

            /**
             * The invalid production error type is used when a
             * production pattern is erroneous. This error is
             * typically caused by referencing undeclared productions,
             * or violating some other production pattern constraint.
             */
            INVALID_PRODUCTION,

            /**
             * The infinite loop error type is used when an infinite
             * loop has been detected in the grammar. One of the
             * productions in the loop will be reported.
             */
            INFINITE_LOOP,

            /**
             * The inherent ambiguity error type is used when the set
             * of production patterns (i.e. the grammar) contains
             * ambiguities that cannot be resolved.
             */
            INHERENT_AMBIGUITY
        }

        private readonly ErrorType _type;
        private readonly string _name;
        private readonly string _info;
        private readonly ArrayList _details;

        public ParserCreationException(ErrorType type,
                                       String info)
            : this(type, null, info)
        {
        }

        public ParserCreationException(ErrorType type,
                                       String name,
                                       String info)
            : this(type, name, info, null)
        {
        }

        public ParserCreationException(ErrorType type,
                                       String name,
                                       String info,
                                       ArrayList details)
        {

            this._type = type;
            this._name = name;
            this._info = info;
            this._details = details;
        }

        public ErrorType Type => _type;

        public ErrorType GetErrorType()
        {
            return Type;
        }

        public string Name => _name;

        public string GetName()
        {
            return Name;
        }

        public string Info => _info;

        public string GetInfo()
        {
            return Info;
        }

        public string Details
        {
            get
            {
                StringBuilder buffer = new StringBuilder();

                if (_details == null)
                {
                    return null;
                }
                for (int i = 0; i < _details.Count; i++)
                {
                    if (i > 0)
                    {
                        buffer.Append(", ");
                        if (i + 1 == _details.Count)
                        {
                            buffer.Append("and ");
                        }
                    }
                    buffer.Append(_details[i]);
                }

                return buffer.ToString();
            }
        }

        public string GetDetails()
        {
            return Details;
        }

        public override string Message
        {
            get
            {
                StringBuilder buffer = new StringBuilder();

                switch (_type)
                {
                    case ErrorType.INVALID_PARSER:
                        buffer.Append("parser is invalid, as ");
                        buffer.Append(_info);
                        break;
                    case ErrorType.INVALID_TOKEN:
                        buffer.Append("token '");
                        buffer.Append(_name);
                        buffer.Append("' is invalid, as ");
                        buffer.Append(_info);
                        break;
                    case ErrorType.INVALID_PRODUCTION:
                        buffer.Append("production '");
                        buffer.Append(_name);
                        buffer.Append("' is invalid, as ");
                        buffer.Append(_info);
                        break;
                    case ErrorType.INFINITE_LOOP:
                        buffer.Append("infinite loop found in production pattern '");
                        buffer.Append(_name);
                        buffer.Append("'");
                        break;
                    case ErrorType.INHERENT_AMBIGUITY:
                        buffer.Append("inherent ambiguity in production '");
                        buffer.Append(_name);
                        buffer.Append("'");
                        if (_info != null)
                        {
                            buffer.Append(" ");
                            buffer.Append(_info);
                        }
                        if (_details != null)
                        {
                            buffer.Append(" starting with ");
                            if (_details.Count > 1)
                            {
                                buffer.Append("tokens ");
                            }
                            else
                            {
                                buffer.Append("token ");
                            }
                            buffer.Append(Details);
                        }
                        break;
                    default:
                        buffer.Append("internal error");
                        break;
                }
                return buffer.ToString();
            }
        }

        public string GetMessage()
        {
            return Message;
        }
    }
}
