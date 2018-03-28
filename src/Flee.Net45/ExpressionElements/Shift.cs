using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection.Emit;
using System.Reflection;
using Flee.ExpressionElements.Base;
using Flee.InternalTypes;


namespace Flee.ExpressionElements
{
    internal class ShiftElement : BinaryExpressionElement
    {
        private ShiftOperation _myOperation;

        public ShiftElement()
        {
        }

        protected override System.Type GetResultType(System.Type leftType, System.Type rightType)
        {
            // Right argument (shift count) must be convertible to int32
            if (ImplicitConverter.EmitImplicitNumericConvert(rightType, typeof(Int32), null) == false)
            {
                return null;
            }

            // Left argument must be an integer type
            if (Utility.IsIntegralType(leftType) == false)
            {
                return null;
            }

            TypeCode tc = Type.GetTypeCode(leftType);

            switch (tc)
            {
                case TypeCode.Byte:
                case TypeCode.SByte:
                case TypeCode.Int16:
                case TypeCode.UInt16:
                case TypeCode.Int32:
                    return typeof(Int32);
                case TypeCode.UInt32:
                    return typeof(UInt32);
                case TypeCode.Int64:
                    return typeof(Int64);
                case TypeCode.UInt64:
                    return typeof(UInt64);
                default:
                    Debug.Assert(false, "unknown left shift operand");
                    return null;
            }
        }

        protected override void GetOperation(object operation)
        {
            _myOperation = (ShiftOperation)operation;
        }

        public override void Emit(FleeILGenerator ilg, IServiceProvider services)
        {
            MyLeftChild.Emit(ilg, services);
            this.EmitShiftCount(ilg, services);
            this.EmitShift(ilg);
        }

        // If the shift count is greater than the number of bits in the number, the result is undefined.
        // So we play it safe and force the shift count to 32/64 bits by ANDing it with the appropriate mask.
        private void EmitShiftCount(FleeILGenerator ilg, IServiceProvider services)
        {
            MyRightChild.Emit(ilg, services);
            TypeCode tc = Type.GetTypeCode(MyLeftChild.ResultType);
            switch (tc)
            {
                case TypeCode.Byte:
                case TypeCode.SByte:
                case TypeCode.Int16:
                case TypeCode.UInt16:
                case TypeCode.Int32:
                case TypeCode.UInt32:
                    ilg.Emit(OpCodes.Ldc_I4_S, Convert.ToSByte(0x1f));
                    break;
                case TypeCode.Int64:
                case TypeCode.UInt64:
                    ilg.Emit(OpCodes.Ldc_I4_S, Convert.ToSByte(0x3f));
                    break;
                default:
                    Debug.Assert(false, "unknown left shift operand");
                    break;
            }

            ilg.Emit(OpCodes.And);
        }

        private void EmitShift(FleeILGenerator ilg)
        {
            TypeCode tc = Type.GetTypeCode(MyLeftChild.ResultType);
            OpCode op = default(OpCode);

            switch (tc)
            {
                case TypeCode.Byte:
                case TypeCode.SByte:
                case TypeCode.Int16:
                case TypeCode.UInt16:
                case TypeCode.Int32:
                case TypeCode.Int64:
                    // Signed operand, emit a left shift or arithmetic right shift
                    if (_myOperation == ShiftOperation.LeftShift)
                    {
                        op = OpCodes.Shl;
                    }
                    else
                    {
                        op = OpCodes.Shr;
                    }
                    break;
                case TypeCode.UInt32:
                case TypeCode.UInt64:
                    // Unsigned operand, emit left shift or logical right shift
                    if (_myOperation == ShiftOperation.LeftShift)
                    {
                        op = OpCodes.Shl;
                    }
                    else
                    {
                        op = OpCodes.Shr_Un;
                    }
                    break;
                default:
                    Debug.Assert(false, "unknown left shift operand");
                    break;
            }

            ilg.Emit(op);
        }
    }
}
