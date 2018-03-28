using System;
using System.Collections;
using System.Diagnostics;
using System.Reflection;
using System.Reflection.Emit;
using Flee.Resources;

namespace Flee.InternalTypes
{
    [Obsolete("Holds various shared utility methods")]
    internal class Utility
    {
        private Utility()
        {
        }

        public static void AssertNotNull(object o, string paramName)
        {
            if (o == null)
            {
                throw new ArgumentNullException(paramName);
            }
        }

        public static void EmitStoreLocal(FleeILGenerator ilg, int index)
        {
            if (index >= 0 & index <= 3)
            {
                switch (index)
                {
                    case 0:
                        ilg.Emit(OpCodes.Stloc_0);
                        break;
                    case 1:
                        ilg.Emit(OpCodes.Stloc_1);
                        break;
                    case 2:
                        ilg.Emit(OpCodes.Stloc_2);
                        break;
                    case 3:
                        ilg.Emit(OpCodes.Stloc_3);
                        break;
                }
            }
            else
            {
                Debug.Assert(index < 256, "local index too large");
                ilg.Emit(OpCodes.Stloc_S, Convert.ToByte(index));
            }
        }

        public static void EmitLoadLocal(FleeILGenerator ilg, int index)
        {
            Debug.Assert(index >= 0, "Invalid index");

            if (index >= 0 & index <= 3)
            {
                switch (index)
                {
                    case 0:
                        ilg.Emit(OpCodes.Ldloc_0);
                        break;
                    case 1:
                        ilg.Emit(OpCodes.Ldloc_1);
                        break;
                    case 2:
                        ilg.Emit(OpCodes.Ldloc_2);
                        break;
                    case 3:
                        ilg.Emit(OpCodes.Ldloc_3);
                        break;
                }
            }
            else
            {
                Debug.Assert(index < 256, "local index too large");
                ilg.Emit(OpCodes.Ldloc_S, Convert.ToByte(index));
            }
        }

        public static void EmitLoadLocalAddress(FleeILGenerator ilg, int index)
        {
            Debug.Assert(index >= 0, "Invalid index");

            if (index <= byte.MaxValue)
            {
                ilg.Emit(OpCodes.Ldloca_S, Convert.ToByte(index));
            }
            else
            {
                ilg.Emit(OpCodes.Ldloca, index);
            }
        }

        public static void EmitArrayLoad(FleeILGenerator ilg, Type elementType)
        {
            TypeCode tc = Type.GetTypeCode(elementType);

            switch (tc)
            {
                case TypeCode.Byte:
                    ilg.Emit(OpCodes.Ldelem_U1);
                    break;
                case TypeCode.SByte:
                case TypeCode.Boolean:
                    ilg.Emit(OpCodes.Ldelem_I1);
                    break;
                case TypeCode.Int16:
                    ilg.Emit(OpCodes.Ldelem_I2);
                    break;
                case TypeCode.UInt16:
                    ilg.Emit(OpCodes.Ldelem_U2);
                    break;
                case TypeCode.Int32:
                    ilg.Emit(OpCodes.Ldelem_I4);
                    break;
                case TypeCode.UInt32:
                    ilg.Emit(OpCodes.Ldelem_U4);
                    break;
                case TypeCode.Int64:
                case TypeCode.UInt64:
                    ilg.Emit(OpCodes.Ldelem_I8);
                    break;
                case TypeCode.Single:
                    ilg.Emit(OpCodes.Ldelem_R4);
                    break;
                case TypeCode.Double:
                    ilg.Emit(OpCodes.Ldelem_R8);
                    break;
                case TypeCode.Object:
                case TypeCode.String:
                    ilg.Emit(OpCodes.Ldelem_Ref);
                    break;
                default:
                    // Must be a non-primitive value type
                    ilg.Emit(OpCodes.Ldelema, elementType);
                    ilg.Emit(OpCodes.Ldobj, elementType);
                    return;
            }
        }

        public static void EmitArrayStore(FleeILGenerator ilg, Type elementType)
        {
            TypeCode tc = Type.GetTypeCode(elementType);

            switch (tc)
            {
                case TypeCode.Byte:
                case TypeCode.SByte:
                case TypeCode.Boolean:
                    ilg.Emit(OpCodes.Stelem_I1);
                    break;
                case TypeCode.Int16:
                case TypeCode.UInt16:
                    ilg.Emit(OpCodes.Stelem_I2);
                    break;
                case TypeCode.Int32:
                case TypeCode.UInt32:
                    ilg.Emit(OpCodes.Stelem_I4);
                    break;
                case TypeCode.Int64:
                case TypeCode.UInt64:
                    ilg.Emit(OpCodes.Stelem_I8);
                    break;
                case TypeCode.Single:
                    ilg.Emit(OpCodes.Stelem_R4);
                    break;
                case TypeCode.Double:
                    ilg.Emit(OpCodes.Stelem_R8);
                    break;
                case TypeCode.Object:
                case TypeCode.String:
                    ilg.Emit(OpCodes.Stelem_Ref);
                    break;
                default:
                    // Must be a non-primitive value type
                    ilg.Emit(OpCodes.Stelem, elementType);
                    break;
            }
        }

        public static void SyncFleeILGeneratorLabels(FleeILGenerator source, FleeILGenerator target)
        {
            while (source.LabelCount != target.LabelCount)
            {
                target.DefineLabel();
            }
        }

        public static bool IsIntegralType(Type t)
        {
            TypeCode tc = Type.GetTypeCode(t);
            switch (tc)
            {
                case TypeCode.Byte:
                case TypeCode.SByte:
                case TypeCode.Int16:
                case TypeCode.UInt16:
                case TypeCode.Int32:
                case TypeCode.UInt32:
                case TypeCode.Int64:
                case TypeCode.UInt64:
                    return true;
                default:
                    return false;
            }
        }

        public static Type GetBitwiseOpType(Type leftType, Type rightType)
        {
            if (IsIntegralType(leftType) == false || IsIntegralType(rightType) == false)
            {
                return null;
            }
            else
            {
                return ImplicitConverter.GetBinaryResultType(leftType, rightType);
            }
        }

        /// <summary>
        /// Find a simple (unary) overloaded operator
        /// </summary>
        /// <param name="name">The name of the operator</param>
        /// <param name="sourceType">The type to convert from</param>
        /// <param name="destType">The type to convert to</param>
        /// <returns>The operator's method or null of no match is found</returns>
        public static MethodInfo GetSimpleOverloadedOperator(string name, Type sourceType, Type destType)
        {
            Hashtable data = new Hashtable();
            data.Add("Name", string.Concat("op_", name));
            data.Add("sourceType", sourceType);
            data.Add("destType", destType);

            const BindingFlags flags = BindingFlags.Public | BindingFlags.Static;

            // Look on the source type
            MemberInfo[] members = sourceType.FindMembers(MemberTypes.Method, flags, SimpleOverloadedOperatorFilter, data);

            if (members.Length == 0)
            {
                // Look on the dest type
                members = destType.FindMembers(MemberTypes.Method, flags, SimpleOverloadedOperatorFilter, data);
            }

            Debug.Assert(members.Length < 2, "Multiple overloaded operators found");

            if (members.Length == 0)
            {
                // No match
                return null;
            }
            else
            {
                return (MethodInfo)members[0];
            }
        }

        /// <summary>
        /// Matches simple overloaded operators
        /// </summary>
        /// <param name="member"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        /// <remarks></remarks>
        private static bool SimpleOverloadedOperatorFilter(MemberInfo member, object value)
        {
            IDictionary data = (IDictionary)value;
            MethodInfo method = (MethodInfo)member;

            bool nameMatch = method.IsSpecialName == true && method.Name.Equals((string)data["Name"], StringComparison.OrdinalIgnoreCase);

            if (nameMatch == false)
            {
                return false;
            }

            bool returnTypeMatch = object.ReferenceEquals(method.ReturnType, (Type)data["destType"]);

            if (returnTypeMatch == false)
            {
                return false;
            }

            ParameterInfo[] parameters = method.GetParameters();
            bool argumentMatch = parameters.Length > 0 && object.ReferenceEquals(parameters[0].ParameterType, (Type)data["sourceType"]);

            return argumentMatch;
        }

        public static MethodInfo GetOverloadedOperator(string name, Type sourceType, Binder binder, params Type[] argumentTypes)
        {
            name = string.Concat("op_", name);
            MethodInfo mi = sourceType.GetMethod(name, BindingFlags.Public | BindingFlags.Static, binder, CallingConventions.Any, argumentTypes, null);

            if (mi == null || mi.IsSpecialName == false)
            {
                return null;
            }
            else
            {
                return mi;
            }
        }

        public static int GetILGeneratorLength(ILGenerator ilg)
        {
            System.Reflection.FieldInfo fi = typeof(ILGenerator).GetField("m_length", BindingFlags.Instance | BindingFlags.NonPublic);
            return (int)fi.GetValue(ilg);
        }

        public static bool IsLongBranch(int startPosition, int endPosition)
        {
            return (endPosition - startPosition) > sbyte.MaxValue;
        }

        public static string FormatList(string[] items)
        {
            string separator = System.Globalization.CultureInfo.CurrentCulture.TextInfo.ListSeparator + " ";
            return string.Join(separator, items);
        }

        public static string GetGeneralErrorMessage(string key, params object[] args)
        {
            string msg = FleeResourceManager.Instance.GetGeneralErrorString(key);
            return string.Format(msg, args);
        }

        public static string GetCompileErrorMessage(string key, params object[] args)
        {
            string msg = FleeResourceManager.Instance.GetCompileErrorString(key);
            return string.Format(msg, args);
        }
    }
}
