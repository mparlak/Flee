using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection.Emit;
using System.Reflection;
using Flee.ExpressionElements.Base;
using Flee.ExpressionElements.Literals.Integral;

using Flee.InternalTypes;
using Flee.PublicTypes;


namespace Flee.ExpressionElements
{
    internal class CompareElement : BinaryExpressionElement
    {
        private LogicalCompareOperation _myOperation;

        public CompareElement()
        {
        }

        public void Initialize(ExpressionElement leftChild, ExpressionElement rightChild, LogicalCompareOperation op)
        {
            MyLeftChild = leftChild;
            MyRightChild = (Int32LiteralElement)rightChild;
            _myOperation = op;
        }

        public void Validate()
        {
            this.ValidateInternal(_myOperation);
        }

        protected override void GetOperation(object operation)
        {
            _myOperation = (LogicalCompareOperation)operation;
        }

        protected override System.Type GetResultType(System.Type leftType, System.Type rightType)
        {
            Type binaryResultType = ImplicitConverter.GetBinaryResultType(leftType, rightType);
            MethodInfo overloadedOperator = this.GetOverloadedCompareOperator();
            bool isEqualityOp = IsOpTypeEqualOrNotEqual(_myOperation);

            // Use our string equality instead of overloaded operator
            if (object.ReferenceEquals(leftType, typeof(string)) & object.ReferenceEquals(rightType, typeof(string)) & isEqualityOp == true)
            {
                // String equality
                return typeof(bool);
            }
            else if ((overloadedOperator != null))
            {
                return overloadedOperator.ReturnType;
            }
            else if ((binaryResultType != null))
            {
                // Comparison of numeric operands
                return typeof(bool);
            }
            else if (object.ReferenceEquals(leftType, typeof(bool)) & object.ReferenceEquals(rightType, typeof(bool)) & isEqualityOp == true)
            {
                // Boolean equality
                return typeof(bool);
            }
            else if (this.AreBothChildrenReferenceTypes() == true & isEqualityOp == true)
            {
                // Comparison of reference types
                return typeof(bool);
            }
            else if (this.AreBothChildrenSameEnum() == true)
            {
                return typeof(bool);
            }
            else
            {
                // Invalid operands
                return null;
            }
        }

        private MethodInfo GetOverloadedCompareOperator()
        {
            string name = GetCompareOperatorName(_myOperation);
            return base.GetOverloadedBinaryOperator(name, _myOperation);
        }

        private static string GetCompareOperatorName(LogicalCompareOperation op)
        {
            switch (op)
            {
                case LogicalCompareOperation.Equal:
                    return "Equality";
                case LogicalCompareOperation.NotEqual:
                    return "Inequality";
                case LogicalCompareOperation.GreaterThan:
                    return "GreaterThan";
                case LogicalCompareOperation.LessThan:
                    return "LessThan";
                case LogicalCompareOperation.GreaterThanOrEqual:
                    return "GreaterThanOrEqual";
                case LogicalCompareOperation.LessThanOrEqual:
                    return "LessThanOrEqual";
                default:
                    Debug.Assert(false, "unknown compare type");
                    return null;
            }
        }

        public override void Emit(FleeILGenerator ilg, IServiceProvider services)
        {
            Type binaryResultType = ImplicitConverter.GetBinaryResultType(MyLeftChild.ResultType, MyRightChild.ResultType);
            MethodInfo overloadedOperator = this.GetOverloadedCompareOperator();

            if (this.AreBothChildrenOfType(typeof(string)))
            {
                // String equality
                MyLeftChild.Emit(ilg, services);
                MyRightChild.Emit(ilg, services);
                EmitStringEquality(ilg, _myOperation, services);
            }
            else if ((overloadedOperator != null))
            {
                base.EmitOverloadedOperatorCall(overloadedOperator, ilg, services);
            }
            else if ((binaryResultType != null))
            {
                // Emit a compare of numeric operands
                EmitChildWithConvert(MyLeftChild, binaryResultType, ilg, services);
                EmitChildWithConvert(MyRightChild, binaryResultType, ilg, services);
                EmitCompareOperation(ilg, _myOperation);
            }
            else if (this.AreBothChildrenOfType(typeof(bool)))
            {
                // Boolean equality
                this.EmitRegular(ilg, services);
            }
            else if (this.AreBothChildrenReferenceTypes() == true)
            {
                // Reference equality
                this.EmitRegular(ilg, services);
            }
            else if (MyLeftChild.ResultType.IsEnum == true & MyRightChild.ResultType.IsEnum == true)
            {
                this.EmitRegular(ilg, services);
            }
            else
            {
                Debug.Fail("unknown operand types");
            }
        }

        private void EmitRegular(FleeILGenerator ilg, IServiceProvider services)
        {
            MyLeftChild.Emit(ilg, services);
            MyRightChild.Emit(ilg, services);
            this.EmitCompareOperation(ilg, _myOperation);
        }

        private static void EmitStringEquality(FleeILGenerator ilg, LogicalCompareOperation op, IServiceProvider services)
        {
            // Get the StringComparison from the options
            ExpressionOptions options = (ExpressionOptions)services.GetService(typeof(ExpressionOptions));
            Int32LiteralElement ic = new Int32LiteralElement((int)options.StringComparison);

            ic.Emit(ilg, services);

            // and emit the method call
            System.Reflection.MethodInfo mi = typeof(string).GetMethod("Equals", new Type[] { typeof(string), typeof(string), typeof(StringComparison) }, null);
            ilg.Emit(OpCodes.Call, mi);

            if (op == LogicalCompareOperation.NotEqual)
            {
                ilg.Emit(OpCodes.Ldc_I4_0);
                ilg.Emit(OpCodes.Ceq);
            }
        }

        private static bool IsOpTypeEqualOrNotEqual(LogicalCompareOperation op)
        {
            return op == LogicalCompareOperation.Equal | op == LogicalCompareOperation.NotEqual;
        }

        private bool AreBothChildrenReferenceTypes()
        {
            return MyLeftChild.ResultType.IsValueType == false & MyRightChild.ResultType.IsValueType == false;
        }

        private bool AreBothChildrenSameEnum()
        {
            return MyLeftChild.ResultType.IsEnum == true && object.ReferenceEquals(MyLeftChild.ResultType, MyRightChild.ResultType);
        }

        /// <summary>
        /// Emit the actual compare
        /// </summary>
        /// <param name="ilg"></param>
        /// <param name="op"></param>
        private void EmitCompareOperation(FleeILGenerator ilg, LogicalCompareOperation op)
        {
            OpCode ltOpcode = this.GetCompareGTLTOpcode(false);
            OpCode gtOpcode = this.GetCompareGTLTOpcode(true);

            switch (op)
            {
                case LogicalCompareOperation.Equal:
                    ilg.Emit(OpCodes.Ceq);
                    break;
                case LogicalCompareOperation.LessThan:
                    ilg.Emit(ltOpcode);
                    break;
                case LogicalCompareOperation.GreaterThan:
                    ilg.Emit(gtOpcode);
                    break;
                case LogicalCompareOperation.NotEqual:
                    ilg.Emit(OpCodes.Ceq);
                    ilg.Emit(OpCodes.Ldc_I4_0);
                    ilg.Emit(OpCodes.Ceq);
                    break;
                case LogicalCompareOperation.LessThanOrEqual:
                    ilg.Emit(gtOpcode);
                    ilg.Emit(OpCodes.Ldc_I4_0);
                    ilg.Emit(OpCodes.Ceq);
                    break;
                case LogicalCompareOperation.GreaterThanOrEqual:
                    ilg.Emit(ltOpcode);
                    ilg.Emit(OpCodes.Ldc_I4_0);
                    ilg.Emit(OpCodes.Ceq);
                    break;
                default:
                    Debug.Fail("Unknown op type");
                    break;
            }
        }

        /// <summary>
        /// Get the correct greater/less than opcode
        /// </summary>
        /// <param name="greaterThan"></param>
        /// <returns></returns>
        private OpCode GetCompareGTLTOpcode(bool greaterThan)
        {
            Type leftType = MyLeftChild.ResultType;

            if (object.ReferenceEquals(leftType, MyRightChild.ResultType))
            {
                if (object.ReferenceEquals(leftType, typeof(UInt32)) | object.ReferenceEquals(leftType, typeof(UInt64)))
                {
                    if (greaterThan == true)
                    {
                        return OpCodes.Cgt_Un;
                    }
                    else
                    {
                        return OpCodes.Clt_Un;
                    }
                }
                else
                {
                    return GetCompareOpcode(greaterThan);
                }
            }
            else
            {
                return GetCompareOpcode(greaterThan);
            }
        }

        private static OpCode GetCompareOpcode(bool greaterThan)
        {
            if (greaterThan == true)
            {
                return OpCodes.Cgt;
            }
            else
            {
                return OpCodes.Clt;
            }
        }
    }
}
