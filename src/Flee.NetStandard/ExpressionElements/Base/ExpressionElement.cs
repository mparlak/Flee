using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection.Emit;
using Flee.InternalTypes;
using Flee.PublicTypes;
using Flee.Resources;

namespace Flee.ExpressionElements.Base
{
    internal abstract class ExpressionElement
    {
        internal ExpressionElement()
        {
        }

        /// <summary>
        /// // All expression elements must be able to emit their IL
        /// </summary>
        /// <param name="ilg"></param>
        /// <param name="services"></param>
        public abstract void Emit(FleeILGenerator ilg, IServiceProvider services);
        /// <summary>
        /// All expression elements must expose the Type they evaluate to
        /// </summary>
        public abstract Type ResultType { get; }

        public override string ToString()
        {
            return this.Name;
        }

        protected void ThrowCompileException(string messageKey, CompileExceptionReason reason, params object[] arguments)
        {
            string messageTemplate = FleeResourceManager.Instance.GetCompileErrorString(messageKey);
            string message = string.Format(messageTemplate, arguments);
            message = string.Concat(this.Name, ": ", message);
            throw new ExpressionCompileException(message, reason);
        }

        protected void ThrowAmbiguousCallException(Type leftType, Type rightType, object operation)
        {
            this.ThrowCompileException(CompileErrorResourceKeys.AmbiguousOverloadedOperator, CompileExceptionReason.AmbiguousMatch, leftType.Name, rightType.Name, operation);
        }

        protected FleeILGenerator CreateTempFleeILGenerator(FleeILGenerator ilgCurrent)
        {
            DynamicMethod dm = new DynamicMethod("temp", typeof(Int32), null, this.GetType());
            return new FleeILGenerator(dm.GetILGenerator(), ilgCurrent.Length, true);
        }

        protected string Name
        {
            get
            {
                string key = this.GetType().Name;
                string value = FleeResourceManager.Instance.GetElementNameString(key);
                Debug.Assert(value != null, $"Element name for '{key}' not in resource file");
                return value;
            }
        }
    }
}
