using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection.Emit;
using Flee.ExpressionElements.Base;
using Flee.InternalTypes;


namespace Flee.ExpressionElements.LogicalBitwise
{
    internal class XorElement : BinaryExpressionElement
    {
        protected override System.Type GetResultType(System.Type leftType, System.Type rightType)
        {
            Type bitwiseType = Utility.GetBitwiseOpType(leftType, rightType);

            if ((bitwiseType != null))
            {
                return bitwiseType;
            }
            else if (this.AreBothChildrenOfType(typeof(bool)) == true)
            {
                return typeof(bool);
            }
            else
            {
                return null;
            }
        }

        public override void Emit(FleeILGenerator ilg, IServiceProvider services)
        {
            Type resultType = this.ResultType;

            MyLeftChild.Emit(ilg, services);
            ImplicitConverter.EmitImplicitConvert(MyLeftChild.ResultType, resultType, ilg);
            MyRightChild.Emit(ilg, services);
            ImplicitConverter.EmitImplicitConvert(MyRightChild.ResultType, resultType, ilg);
            ilg.Emit(OpCodes.Xor);
        }


        protected override void GetOperation(object operation)
        {
        }
    }
}
