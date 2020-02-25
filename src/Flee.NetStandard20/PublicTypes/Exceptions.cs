using System;
using Flee.InternalTypes;
using Flee.Parsing.grammatica_1._5.alpha2.PerCederberg.Grammatica.Runtime;
using Flee.Resources;

namespace Flee.PublicTypes
{
    public enum CompileExceptionReason
    {
        SyntaxError,
        ConstantOverflow,
        TypeMismatch,
        UndefinedName,
        FunctionHasNoReturnValue,
        InvalidExplicitCast,
        AmbiguousMatch,
        AccessDenied,
        InvalidFormat
    }

    /// <summary>
    /// 
    /// </summary>
    [Serializable()]
    public sealed class ExpressionCompileException : Exception
    {
        private readonly CompileExceptionReason _myReason;
        public object[] Arguments { get; }

        internal ExpressionCompileException(string message, CompileExceptionReason reason, object[] arguments) : base(message)
        {
            Arguments = arguments;
            _myReason = reason;
        }

        internal ExpressionCompileException(ParserLogException parseException) : base(string.Empty, parseException)
        {
            _myReason = CompileExceptionReason.SyntaxError;
        }

        private ExpressionCompileException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) : base(info, context)
        {
            _myReason = (CompileExceptionReason)info.GetInt32("Reason");
        }

        public override void GetObjectData(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("Reason", Convert.ToInt32(_myReason));
        }

        public override string Message
        {
            get
            {
                if (_myReason == CompileExceptionReason.SyntaxError)
                {
                    Exception innerEx = this.InnerException;
                    string msg = $"{Utility.GetCompileErrorMessage(CompileErrorResourceKeys.SyntaxError)}: {innerEx.Message}";
                    return msg;
                }
                else
                {
                    return base.Message;
                }
            }
        }

        public CompileExceptionReason Reason => _myReason;
    }

    /// <summary>
    /// Exception, which occured during expression evaluation.
    /// </summary>
    [Serializable]
    public sealed class ExpressionEvaluationException : Exception
    {
        public ExpressionEvaluationException(string message) : base(message)
        {
            
        }
    }
}
