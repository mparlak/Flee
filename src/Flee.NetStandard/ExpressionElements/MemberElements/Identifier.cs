using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Reflection.Emit;
using System.ComponentModel;
using Flee.CalcEngine.PublicTypes;
using Flee.ExpressionElements.Base;
using Flee.ExpressionElements.Base.Literals;
using Flee.ExpressionElements.Literals;
using Flee.ExpressionElements.Literals.Integral;
using Flee.ExpressionElements.Literals.Real;
using Flee.InternalTypes;
using Flee.PublicTypes;
using Flee.Resources;


namespace Flee.ExpressionElements.MemberElements
{
    [Obsolete("Represents an identifier")]
    internal class IdentifierElement : MemberElement
    {
        private FieldInfo _myField;
        private PropertyInfo _myProperty;
        private PropertyDescriptor _myPropertyDescriptor;
        private Type _myVariableType;
        private Type _myCalcEngineReferenceType;
        public IdentifierElement(string name)
        {
            this.MyName = name;
        }

        protected override void ResolveInternal()
        {
            // Try to bind to a field or property
            if (this.ResolveFieldProperty(MyPrevious) == true)
            {
                this.AddReferencedVariable(MyPrevious);
                return;
            }

            // Try to find a variable with our name
            _myVariableType = MyContext.Variables.GetVariableTypeInternal(MyName);

            // Variables are only usable as the first element
            if (MyPrevious == null && (_myVariableType != null))
            {
                this.AddReferencedVariable(MyPrevious);
                return;
            }

            CalculationEngine ce = MyContext.CalculationEngine;

            if ((ce != null))
            {
                ce.AddDependency(MyName, MyContext);
                _myCalcEngineReferenceType = ce.ResolveTailType(MyName);
                return;
            }

            if (MyPrevious == null)
            {
                base.ThrowCompileException(CompileErrorResourceKeys.NoIdentifierWithName, CompileExceptionReason.UndefinedName, MyName);
            }
            else
            {
                base.ThrowCompileException(CompileErrorResourceKeys.NoIdentifierWithNameOnType, CompileExceptionReason.UndefinedName, MyName, MyPrevious.TargetType.Name);
            }
        }

        private bool ResolveFieldProperty(MemberElement previous)
        {
            MemberInfo[] members = this.GetMembers(MemberTypes.Field | MemberTypes.Property);

            // Keep only the ones which are accessible
            members = this.GetAccessibleMembers(members);

            if (members.Length == 0)
            {
                // No accessible members; try to resolve a virtual property
                return this.ResolveVirtualProperty(previous);
            }
            else if (members.Length > 1)
            {
                // More than one accessible member
                if (previous == null)
                {
                    base.ThrowCompileException(CompileErrorResourceKeys.IdentifierIsAmbiguous, CompileExceptionReason.AmbiguousMatch, MyName);
                }
                else
                {
                    base.ThrowCompileException(CompileErrorResourceKeys.IdentifierIsAmbiguousOnType, CompileExceptionReason.AmbiguousMatch, MyName, previous.TargetType.Name);
                }
            }
            else
            {
                // Only one member; bind to it
                _myField = members[0] as FieldInfo;
                if ((_myField != null))
                {
                    return true;
                }

                // Assume it must be a property
                _myProperty = (PropertyInfo)members[0];
                return true;
            }

            return false;
        }

        private bool ResolveVirtualProperty(MemberElement previous)
        {
            if (previous == null)
            {
                // We can't use virtual properties if we are the first element
                return false;
            }

            PropertyDescriptorCollection coll = TypeDescriptor.GetProperties(previous.ResultType);
            _myPropertyDescriptor = coll.Find(MyName, true);
            return (_myPropertyDescriptor != null);
        }

        private void AddReferencedVariable(MemberElement previous)
        {
            if ((previous != null))
            {
                return;
            }

            if ((_myVariableType != null) || MyOptions.IsOwnerType(this.MemberOwnerType) == true)
            {
                ExpressionInfo info = (ExpressionInfo)MyServices.GetService(typeof(ExpressionInfo));
                info.AddReferencedVariable(MyName);
            }
        }

        public override void Emit(FleeILGenerator ilg, IServiceProvider services)
        {
            base.Emit(ilg, services);

            this.EmitFirst(ilg);

            if ((_myCalcEngineReferenceType != null))
            {
                this.EmitReferenceLoad(ilg);
            }
            else if ((_myVariableType != null))
            {
                this.EmitVariableLoad(ilg);
            }
            else if ((_myField != null))
            {
                this.EmitFieldLoad(_myField, ilg, services);
            }
            else if ((_myPropertyDescriptor != null))
            {
                this.EmitVirtualPropertyLoad(ilg);
            }
            else
            {
                this.EmitPropertyLoad(_myProperty, ilg);
            }
        }

        private void EmitReferenceLoad(FleeILGenerator ilg)
        {
            ilg.Emit(OpCodes.Ldarg_1);
            MyContext.CalculationEngine.EmitLoad(MyName, ilg);
        }

        private void EmitFirst(FleeILGenerator ilg)
        {
            if ((MyPrevious != null))
            {
                return;
            }

            bool isVariable = (_myVariableType != null);

            if (isVariable == true)
            {
                // Load variables
                EmitLoadVariables(ilg);
            }
            else if (MyOptions.IsOwnerType(this.MemberOwnerType) == true & this.IsStatic == false)
            {
                this.EmitLoadOwner(ilg);
            }
        }

        private void EmitVariableLoad(FleeILGenerator ilg)
        {
            MethodInfo mi = VariableCollection.GetVariableLoadMethod(_myVariableType);
            ilg.Emit(OpCodes.Ldstr, MyName);
            this.EmitMethodCall(mi, ilg);
        }

        private void EmitFieldLoad(System.Reflection.FieldInfo fi, FleeILGenerator ilg, IServiceProvider services)
        {
            if (fi.IsLiteral == true)
            {
                EmitLiteral(fi, ilg, services);
            }
            else if (this.ResultType.IsValueType == true & this.NextRequiresAddress == true)
            {
                EmitLdfld(fi, true, ilg);
            }
            else
            {
                EmitLdfld(fi, false, ilg);
            }
        }

        private static void EmitLdfld(System.Reflection.FieldInfo fi, bool indirect, FleeILGenerator ilg)
        {
            if (fi.IsStatic == true)
            {
                if (indirect == true)
                {
                    ilg.Emit(OpCodes.Ldsflda, fi);
                }
                else
                {
                    ilg.Emit(OpCodes.Ldsfld, fi);
                }
            }
            else
            {
                if (indirect == true)
                {
                    ilg.Emit(OpCodes.Ldflda, fi);
                }
                else
                {
                    ilg.Emit(OpCodes.Ldfld, fi);
                }
            }
        }

        /// <summary>
        /// Emit the load of a constant field.  We can't emit a ldsfld/ldfld of a constant so we have to get its value
        /// and then emit a ldc.
        /// </summary>
        /// <param name="fi"></param>
        /// <param name="ilg"></param>
        /// <param name="services"></param>
        private static void EmitLiteral(System.Reflection.FieldInfo fi, FleeILGenerator ilg, IServiceProvider services)
        {
            object value = fi.GetValue(null);
            Type t = value.GetType();
            TypeCode code = Type.GetTypeCode(t);
            LiteralElement elem = default(LiteralElement);

            switch (code)
            {
                case TypeCode.Char:
                case TypeCode.Byte:
                case TypeCode.SByte:
                case TypeCode.Int16:
                case TypeCode.UInt16:
                case TypeCode.Int32:
                    elem = new Int32LiteralElement(System.Convert.ToInt32(value));
                    break;
                case TypeCode.UInt32:
                    elem = new UInt32LiteralElement((UInt32)value);
                    break;
                case TypeCode.Int64:
                    elem = new Int64LiteralElement((Int64)value);
                    break;
                case TypeCode.UInt64:
                    elem = new UInt64LiteralElement((UInt64)value);
                    break;
                case TypeCode.Double:
                    elem = new DoubleLiteralElement((double)value);
                    break;
                case TypeCode.Single:
                    elem = new SingleLiteralElement((float)value);
                    break;
                case TypeCode.Boolean:
                    elem = new BooleanLiteralElement((bool)value);
                    break;
                case TypeCode.String:
                    elem = new StringLiteralElement((string)value);
                    break;
                default:
                    elem = null;
                    Debug.Fail("Unsupported constant type");
                    break;
            }

            elem.Emit(ilg, services);
        }

        private void EmitPropertyLoad(System.Reflection.PropertyInfo pi, FleeILGenerator ilg)
        {
            System.Reflection.MethodInfo getter = pi.GetGetMethod(true);
            base.EmitMethodCall(getter, ilg);
        }

        /// <summary>
        /// Load a PropertyDescriptor based property
        /// </summary>
        /// <param name="ilg"></param>
        private void EmitVirtualPropertyLoad(FleeILGenerator ilg)
        {
            // The previous value is already on the top of the stack but we need it at the bottom

            // Get a temporary local index
            int index = ilg.GetTempLocalIndex(MyPrevious.ResultType);

            // Store the previous value there
            Utility.EmitStoreLocal(ilg, index);

            // Load the variable collection
            EmitLoadVariables(ilg);
            // Load the property name
            ilg.Emit(OpCodes.Ldstr, MyName);

            // Load the previous value and convert it to object
            Utility.EmitLoadLocal(ilg, index);
            ImplicitConverter.EmitImplicitConvert(MyPrevious.ResultType, typeof(object), ilg);

            // Call the method to get the actual value
            MethodInfo mi = VariableCollection.GetVirtualPropertyLoadMethod(this.ResultType);
            this.EmitMethodCall(mi, ilg);
        }

        private Type MemberOwnerType
        {
            get
            {
                if ((_myField != null))
                {
                    return _myField.ReflectedType;
                }
                else if ((_myPropertyDescriptor != null))
                {
                    return _myPropertyDescriptor.ComponentType;
                }
                else if ((_myProperty != null))
                {
                    return _myProperty.ReflectedType;
                }
                else
                {
                    return null;
                }
            }
        }

        public override System.Type ResultType
        {
            get
            {
                if ((_myCalcEngineReferenceType != null))
                {
                    return _myCalcEngineReferenceType;
                }
                else if ((_myVariableType != null))
                {
                    return _myVariableType;
                }
                else if ((_myPropertyDescriptor != null))
                {
                    return _myPropertyDescriptor.PropertyType;
                }
                else if ((_myField != null))
                {
                    return _myField.FieldType;
                }
                else
                {
                    MethodInfo mi = _myProperty.GetGetMethod(true);
                    return mi.ReturnType;
                }
            }
        }

        protected override bool RequiresAddress => _myPropertyDescriptor == null;

        protected override bool IsPublic
        {
            get
            {
                if ((_myVariableType != null) | (_myCalcEngineReferenceType != null))
                {
                    return true;
                }
                else if ((_myVariableType != null))
                {
                    return true;
                }
                else if ((_myPropertyDescriptor != null))
                {
                    return true;
                }
                else if ((_myField != null))
                {
                    return _myField.IsPublic;
                }
                else
                {
                    MethodInfo mi = _myProperty.GetGetMethod(true);
                    return mi.IsPublic;
                }
            }
        }

        protected override bool SupportsStatic
        {
            get
            {
                if ((_myVariableType != null))
                {
                    // Variables never support static
                    return false;
                }
                else if ((_myPropertyDescriptor != null))
                {
                    // Neither do virtual properties
                    return false;
                }
                else if (MyOptions.IsOwnerType(this.MemberOwnerType) == true && MyPrevious == null)
                {
                    // Owner members support static if we are the first element
                    return true;
                }
                else
                {
                    // Support static if we are the first (ie: we are a static import)
                    return MyPrevious == null;
                }
            }
        }

        protected override bool SupportsInstance
        {
            get
            {
                if ((_myVariableType != null))
                {
                    // Variables always support instance
                    return true;
                }
                else if ((_myPropertyDescriptor != null))
                {
                    // So do virtual properties
                    return true;
                }
                else if (MyOptions.IsOwnerType(this.MemberOwnerType) == true && MyPrevious == null)
                {
                    // Owner members support instance if we are the first element
                    return true;
                }
                else
                {
                    // We always support instance if we are not the first element
                    return (MyPrevious != null);
                }
            }
        }

        public override bool IsStatic
        {
            get
            {
                if ((_myVariableType != null) | (_myCalcEngineReferenceType != null))
                {
                    return false;
                }
                else if ((_myVariableType != null))
                {
                    return false;
                }
                else if ((_myField != null))
                {
                    return _myField.IsStatic;
                }
                else if ((_myPropertyDescriptor != null))
                {
                    return false;
                }
                else
                {
                    MethodInfo mi = _myProperty.GetGetMethod(true);
                    return mi.IsStatic;
                }
            }
        }
    }
}
