using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using Flee.ExpressionElements.Base;
using Flee.PublicTypes;
using Flee.Resources;

namespace Flee.ExpressionElements.Base
{
    internal abstract class UnaryElement : ExpressionElement
    {

        protected ExpressionElement MyChild;

        private Type _myResultType;
        public void SetChild(ExpressionElement child)
        {
            MyChild = child;
            _myResultType = this.GetResultType(child.ResultType);

            if (_myResultType == null)
            {
                base.ThrowCompileException(CompileErrorResourceKeys.OperationNotDefinedForType, CompileExceptionReason.TypeMismatch, MyChild.ResultType.Name);
            }
        }

        protected abstract Type GetResultType(Type childType);

        public override System.Type ResultType => _myResultType;
    }

}
