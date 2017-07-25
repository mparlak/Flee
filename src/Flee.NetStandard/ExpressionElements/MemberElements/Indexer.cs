using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Reflection.Emit;
using Flee.ExpressionElements.Base;
using Flee.ExpressionElements.MemberElements;
using Flee.InternalTypes;
using Flee.PublicTypes;
using Flee.Resources;


namespace Flee.ExpressionElements.MemberElements
{
    [Obsolete("Element representing an array index")]
    internal class IndexerElement : MemberElement
    {
        private ExpressionElement _myIndexerElement;

        private readonly ArgumentList _myIndexerElements;
        public IndexerElement(ArgumentList indexer)
        {
            _myIndexerElements = indexer;
        }

        protected override void ResolveInternal()
        {
            // Are we are indexing on an array?
            Type target = MyPrevious.TargetType;

            // Yes, so setup for an array index
            if (target.IsArray == true)
            {
                this.SetupArrayIndexer();
                return;
            }

            // Not an array, so try to find an indexer on the type
            if (this.FindIndexer(target) == false)
            {
                base.ThrowCompileException(CompileErrorResourceKeys.TypeNotArrayAndHasNoIndexerOfType, CompileExceptionReason.TypeMismatch, target.Name, _myIndexerElements);
            }
        }

        private void SetupArrayIndexer()
        {
            _myIndexerElement = _myIndexerElements[0];

            if (_myIndexerElements.Count > 1)
            {
                base.ThrowCompileException(CompileErrorResourceKeys.MultiArrayIndexNotSupported, CompileExceptionReason.TypeMismatch);
            }
            else if (ImplicitConverter.EmitImplicitConvert(_myIndexerElement.ResultType, typeof(Int32), null) == false)
            {
                base.ThrowCompileException(CompileErrorResourceKeys.ArrayIndexersMustBeOfType, CompileExceptionReason.TypeMismatch, typeof(Int32).Name);
            }
        }

        private bool FindIndexer(Type targetType)
        {
            // Get the default members
            MemberInfo[] members = targetType.GetDefaultMembers();

            List<MethodInfo> methods = new List<MethodInfo>();

            // Use the first one that's valid for our indexer type
            foreach (MemberInfo mi in members)
            {
                PropertyInfo pi = mi as PropertyInfo;
                if ((pi != null))
                {
                    methods.Add(pi.GetGetMethod(true));
                }
            }

            FunctionCallElement func = new FunctionCallElement("Indexer", methods.ToArray(), _myIndexerElements);
            func.Resolve(MyServices);
            _myIndexerElement = func;

            return true;
        }

        public override void Emit(FleeILGenerator ilg, IServiceProvider services)
        {
            base.Emit(ilg, services);

            if (this.IsArray == true)
            {
                this.EmitArrayLoad(ilg, services);
            }
            else
            {
                this.EmitIndexer(ilg, services);
            }
        }

        private void EmitArrayLoad(FleeILGenerator ilg, IServiceProvider services)
        {
            _myIndexerElement.Emit(ilg, services);
            ImplicitConverter.EmitImplicitConvert(_myIndexerElement.ResultType, typeof(Int32), ilg);

            Type elementType = this.ResultType;

            if (elementType.IsValueType == false)
            {
                // Simple reference load
                ilg.Emit(OpCodes.Ldelem_Ref);
            }
            else
            {
                this.EmitValueTypeArrayLoad(ilg, elementType);
            }
        }

        private void EmitValueTypeArrayLoad(FleeILGenerator ilg, Type elementType)
        {
            if (this.NextRequiresAddress == true)
            {
                ilg.Emit(OpCodes.Ldelema, elementType);
            }
            else
            {
                Utility.EmitArrayLoad(ilg, elementType);
            }
        }

        private void EmitIndexer(FleeILGenerator ilg, IServiceProvider services)
        {
            FunctionCallElement func = (FunctionCallElement)_myIndexerElement;
            func.EmitFunctionCall(this.NextRequiresAddress, ilg, services);
        }

        private Type ArrayType
        {
            get
            {
                if (this.IsArray == true)
                {
                    return MyPrevious.TargetType;
                }
                else
                {
                    return null;
                }
            }
        }

        private bool IsArray => MyPrevious.TargetType.IsArray;

        protected override bool RequiresAddress => this.IsArray == false;

        public override System.Type ResultType
        {
            get
            {
                if (this.IsArray == true)
                {
                    return this.ArrayType.GetElementType();
                }
                else
                {
                    return _myIndexerElement.ResultType;
                }
            }
        }

        protected override bool IsPublic
        {
            get
            {
                if (this.IsArray == true)
                {
                    return true;
                }
                else
                {
                    return IsElementPublic((MemberElement)_myIndexerElement);
                }
            }
        }

        public override bool IsStatic => false;
    }
}
