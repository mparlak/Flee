using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection.Emit;
using System.Reflection;
using Flee.ExpressionElements.Base;
using Flee.InternalTypes;
using Flee.PublicTypes;
using Flee.Resources;

namespace Flee.ExpressionElements
{
    internal class CastElement : ExpressionElement
    {
        private readonly ExpressionElement _myCastExpression;
        private readonly Type _myDestType;
        public CastElement(ExpressionElement castExpression, string[] destTypeParts, bool isArray, IServiceProvider services)
        {
            _myCastExpression = castExpression;

            _myDestType = GetDestType(destTypeParts, services);

            if (_myDestType == null)
            {
                base.ThrowCompileException(CompileErrorResourceKeys.CouldNotResolveType, CompileExceptionReason.UndefinedName, GetDestTypeString(destTypeParts, isArray));
            }

            if (isArray == true)
            {
                _myDestType = _myDestType.MakeArrayType();
            }

            if (this.IsValidCast(_myCastExpression.ResultType, _myDestType) == false)
            {
                this.ThrowInvalidCastException();
            }
        }

        private static string GetDestTypeString(string[] parts, bool isArray)
        {
            string s = string.Join(".", parts);

            if (isArray == true)
            {
                s = s + "[]";
            }

            return s;
        }

        /// <summary>
        /// Resolve the type we are casting to
        /// </summary>
        /// <param name="destTypeParts"></param>
        /// <param name="services"></param>
        /// <returns></returns>
        private static Type GetDestType(string[] destTypeParts, IServiceProvider services)
        {
            ExpressionContext context = (ExpressionContext)services.GetService(typeof(ExpressionContext));

            Type t = null;

            // Try to find a builtin type with the name
            if (destTypeParts.Length == 1)
            {
                t = ExpressionImports.GetBuiltinType(destTypeParts[0]);
            }

            if ((t != null))
            {
                return t;
            }

            // Try to find the type in an import
            t = context.Imports.FindType(destTypeParts);

            if ((t != null))
            {
                return t;
            }

            return null;
        }

        private bool IsValidCast(Type sourceType, Type destType)
        {
            if (object.ReferenceEquals(sourceType, destType))
            {
                // Identity cast always succeeds
                return true;
            }
            else if (destType.IsAssignableFrom(sourceType) == true)
            {
                // Cast is already implicitly valid
                return true;
            }
            else if (ImplicitConverter.EmitImplicitConvert(sourceType, destType, null) == true)
            {
                // Cast is already implicitly valid
                return true;
            }
            else if (IsCastableNumericType(sourceType) & IsCastableNumericType(destType))
            {
                // Explicit cast of numeric types always succeeds
                return true;
            }
            else if (sourceType.IsEnum == true | destType.IsEnum == true)
            {
                return this.IsValidExplicitEnumCast(sourceType, destType);
            }
            else if ((this.GetExplictOverloadedOperator(sourceType, destType) != null))
            {
                // Overloaded explict cast exists
                return true;
            }

            if (sourceType.IsValueType == true)
            {
                // If we get here then the cast always fails since we are either casting one value type to another
                // or a value type to an invalid reference type
                return false;
            }
            else
            {
                if (destType.IsValueType == true)
                {
                    // Reference type to value type
                    // Can only succeed if the reference type is a base of the value type or
                    // it is one of the interfaces the value type implements
                    Type[] interfaces = destType.GetInterfaces();
                    return IsBaseType(destType, sourceType) == true | System.Array.IndexOf(interfaces, sourceType) != -1;
                }
                else
                {
                    // Reference type to reference type
                    return this.IsValidExplicitReferenceCast(sourceType, destType);
                }
            }
        }

        private MethodInfo GetExplictOverloadedOperator(Type sourceType, Type destType)
        {
            ExplicitOperatorMethodBinder binder = new ExplicitOperatorMethodBinder(destType, sourceType);

            // Look for an operator on the source type and dest types
            MethodInfo miSource = Utility.GetOverloadedOperator("Explicit", sourceType, binder, sourceType);
            MethodInfo miDest = Utility.GetOverloadedOperator("Explicit", destType, binder, sourceType);

            if (miSource == null & miDest == null)
            {
                return null;
            }
            else if (miSource == null)
            {
                return miDest;
            }
            else if (miDest == null)
            {
                return miSource;
            }
            else
            {
                base.ThrowAmbiguousCallException(sourceType, destType, "Explicit");
                return null;
            }
        }

        private bool IsValidExplicitEnumCast(Type sourceType, Type destType)
        {
            sourceType = GetUnderlyingEnumType(sourceType);
            destType = GetUnderlyingEnumType(destType);
            return this.IsValidCast(sourceType, destType);
        }

        private bool IsValidExplicitReferenceCast(Type sourceType, Type destType)
        {
            Debug.Assert(sourceType.IsValueType == false & destType.IsValueType == false, "expecting reference types");

            if (object.ReferenceEquals(sourceType, typeof(object)))
            {
                // From object to any other reference-type
                return true;
            }
            else if (sourceType.IsArray == true & destType.IsArray == true)
            {
                // From an array-type S with an element type SE to an array-type T with an element type TE,
                // provided all of the following are true:

                // S and T have the same number of dimensions
                if (sourceType.GetArrayRank() != destType.GetArrayRank())
                {
                    return false;
                }
                else
                {
                    Type SE = sourceType.GetElementType();
                    Type TE = destType.GetElementType();

                    // Both SE and TE are reference-types
                    if (SE.IsValueType == true | TE.IsValueType == true)
                    {
                        return false;
                    }
                    else
                    {
                        // An explicit reference conversion exists from SE to TE
                        return this.IsValidExplicitReferenceCast(SE, TE);
                    }
                }
            }
            else if (sourceType.IsClass == true & destType.IsClass == true)
            {
                // From any class-type S to any class-type T, provided S is a base class of T
                return IsBaseType(destType, sourceType);
            }
            else if (sourceType.IsClass == true & destType.IsInterface == true)
            {
                // From any class-type S to any interface-type T, provided S is not sealed and provided S does not implement T
                return sourceType.IsSealed == false & ImplementsInterface(sourceType, destType) == false;
            }
            else if (sourceType.IsInterface == true & destType.IsClass == true)
            {
                // From any interface-type S to any class-type T, provided T is not sealed or provided T implements S.
                return destType.IsSealed == false | ImplementsInterface(destType, sourceType) == true;
            }
            else if (sourceType.IsInterface == true & destType.IsInterface == true)
            {
                // From any interface-type S to any interface-type T, provided S is not derived from T
                return ImplementsInterface(sourceType, destType) == false;
            }
            else
            {
                Debug.Assert(false, "unknown explicit cast");
            }

            return false;
        }

        private static bool IsBaseType(Type target, Type potentialBase)
        {
            Type current = target;
            while ((current != null))
            {
                if (object.ReferenceEquals(current, potentialBase))
                {
                    return true;
                }
                current = current.BaseType;
            }
            return false;
        }

        private static bool ImplementsInterface(Type target, Type interfaceType)
        {
            Type[] interfaces = target.GetInterfaces();
            return System.Array.IndexOf(interfaces, interfaceType) != -1;
        }

        private void ThrowInvalidCastException()
        {
            base.ThrowCompileException(CompileErrorResourceKeys.CannotConvertType, CompileExceptionReason.InvalidExplicitCast, _myCastExpression.ResultType.Name, _myDestType.Name);
        }

        private static bool IsCastableNumericType(Type t)
        {
            return t.IsPrimitive == true & (!object.ReferenceEquals(t, typeof(bool)));
        }

        private static Type GetUnderlyingEnumType(Type t)
        {
            if (t.IsEnum == true)
            {
                return System.Enum.GetUnderlyingType(t);
            }
            else
            {
                return t;
            }
        }

        public override void Emit(FleeILGenerator ilg, IServiceProvider services)
        {
            _myCastExpression.Emit(ilg, services);

            Type sourceType = _myCastExpression.ResultType;
            Type destType = _myDestType;

            this.EmitCast(ilg, sourceType, destType, services);
        }

        private void EmitCast(FleeILGenerator ilg, Type sourceType, Type destType, IServiceProvider services)
        {
            MethodInfo explicitOperator = this.GetExplictOverloadedOperator(sourceType, destType);

            if (object.ReferenceEquals(sourceType, destType))
            {
                // Identity cast; do nothing
                return;
            }
            else if ((explicitOperator != null))
            {
                ilg.Emit(OpCodes.Call, explicitOperator);
            }
            else if (sourceType.IsEnum == true | destType.IsEnum == true)
            {
                this.EmitEnumCast(ilg, sourceType, destType, services);
            }
            else if (ImplicitConverter.EmitImplicitConvert(sourceType, destType, ilg) == true)
            {
                // Implicit numeric cast; do nothing
                return;
            }
            else if (IsCastableNumericType(sourceType) & IsCastableNumericType(destType))
            {
                // Explicit numeric cast
                EmitExplicitNumericCast(ilg, sourceType, destType, services);
            }
            else if (sourceType.IsValueType == true)
            {
                Debug.Assert(destType.IsValueType == false, "expecting reference type");
                ilg.Emit(OpCodes.Box, sourceType);
            }
            else
            {
                if (destType.IsValueType == true)
                {
                    // Reference type to value type
                    ilg.Emit(OpCodes.Unbox_Any, destType);
                }
                else
                {
                    // Reference type to reference type
                    if (destType.IsAssignableFrom(sourceType) == false)
                    {
                        // Only emit cast if it is an explicit cast
                        ilg.Emit(OpCodes.Castclass, destType);
                    }
                }
            }
        }

        private void EmitEnumCast(FleeILGenerator ilg, Type sourceType, Type destType, IServiceProvider services)
        {
            if (destType.IsValueType == false)
            {
                ilg.Emit(OpCodes.Box, sourceType);
            }
            else if (sourceType.IsValueType == false)
            {
                ilg.Emit(OpCodes.Unbox_Any, destType);
            }
            else
            {
                sourceType = GetUnderlyingEnumType(sourceType);
                destType = GetUnderlyingEnumType(destType);
                this.EmitCast(ilg, sourceType, destType, services);
            }
        }

        private static void EmitExplicitNumericCast(FleeILGenerator ilg, Type sourceType, Type destType, IServiceProvider services)
        {
            TypeCode desttc = Type.GetTypeCode(destType);
            TypeCode sourcetc = Type.GetTypeCode(sourceType);
            bool unsigned = IsUnsignedType(sourceType);
            ExpressionOptions options = (ExpressionOptions)services.GetService(typeof(ExpressionOptions));
            bool @checked = options.Checked;
            OpCode op = OpCodes.Nop;

            switch (desttc)
            {
                case TypeCode.SByte:
                    if (unsigned == true & @checked == true)
                    {
                        op = OpCodes.Conv_Ovf_I1_Un;
                    }
                    else if (@checked == true)
                    {
                        op = OpCodes.Conv_Ovf_I1;
                    }
                    else
                    {
                        op = OpCodes.Conv_I1;
                    }
                    break;
                case TypeCode.Byte:
                    if (unsigned == true & @checked == true)
                    {
                        op = OpCodes.Conv_Ovf_U1_Un;
                    }
                    else if (@checked == true)
                    {
                        op = OpCodes.Conv_Ovf_U1;
                    }
                    else
                    {
                        op = OpCodes.Conv_U1;
                    }
                    break;
                case TypeCode.Int16:
                    if (unsigned == true & @checked == true)
                    {
                        op = OpCodes.Conv_Ovf_I2_Un;
                    }
                    else if (@checked == true)
                    {
                        op = OpCodes.Conv_Ovf_I2;
                    }
                    else
                    {
                        op = OpCodes.Conv_I2;
                    }
                    break;
                case TypeCode.UInt16:
                    if (unsigned == true & @checked == true)
                    {
                        op = OpCodes.Conv_Ovf_U2_Un;
                    }
                    else if (@checked == true)
                    {
                        op = OpCodes.Conv_Ovf_U2;
                    }
                    else
                    {
                        op = OpCodes.Conv_U2;
                    }
                    break;
                case TypeCode.Int32:
                    if (unsigned == true & @checked == true)
                    {
                        op = OpCodes.Conv_Ovf_I4_Un;
                    }
                    else if (@checked == true)
                    {
                        op = OpCodes.Conv_Ovf_I4;
                    }
                    else if (sourcetc != TypeCode.UInt32)
                    {
                        // Don't need to emit a convert for this case since, to the CLR, it is the same data type
                        op = OpCodes.Conv_I4;
                    }
                    break;
                case TypeCode.UInt32:
                    if (unsigned == true & @checked == true)
                    {
                        op = OpCodes.Conv_Ovf_U4_Un;
                    }
                    else if (@checked == true)
                    {
                        op = OpCodes.Conv_Ovf_U4;
                    }
                    else if (sourcetc != TypeCode.Int32)
                    {
                        op = OpCodes.Conv_U4;
                    }
                    break;
                case TypeCode.Int64:
                    if (unsigned == true & @checked == true)
                    {
                        op = OpCodes.Conv_Ovf_I8_Un;
                    }
                    else if (@checked == true)
                    {
                        op = OpCodes.Conv_Ovf_I8;
                    }
                    else if (sourcetc != TypeCode.UInt64)
                    {
                        op = OpCodes.Conv_I8;
                    }
                    break;
                case TypeCode.UInt64:
                    if (unsigned == true & @checked == true)
                    {
                        op = OpCodes.Conv_Ovf_U8_Un;
                    }
                    else if (@checked == true)
                    {
                        op = OpCodes.Conv_Ovf_U8;
                    }
                    else if (sourcetc != TypeCode.Int64)
                    {
                        op = OpCodes.Conv_U8;
                    }
                    break;
                case TypeCode.Single:
                    op = OpCodes.Conv_R4;
                    break;
                default:
                    Debug.Assert(false, "Unknown cast dest type");
                    break;
            }

            if (op.Equals(OpCodes.Nop) == false)
            {
                ilg.Emit(op);
            }
        }

        private static bool IsUnsignedType(Type t)
        {
            TypeCode tc = Type.GetTypeCode(t);
            switch (tc)
            {
                case TypeCode.Byte:
                case TypeCode.UInt16:
                case TypeCode.UInt32:
                case TypeCode.UInt64:
                    return true;
                default:
                    return false;
            }
        }

        public override System.Type ResultType => _myDestType;
    }
}
