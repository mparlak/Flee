using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection.Emit;
using System.Reflection;
using Flee.ExpressionElements;
using Flee.ExpressionElements.Base;

using Flee.InternalTypes;
using Flee.PublicTypes;
using Flee.Resources;


namespace Flee.ExpressionElements
{
    internal class InElement : ExpressionElement
    {
        // Element we will search for
        private ExpressionElement MyOperand;
        // Elements we will compare against
        private List<ExpressionElement> MyArguments;
        // Collection to look in
        private ExpressionElement MyTargetCollectionElement;
        // Type of the collection

        private Type MyTargetCollectionType;
        // Initialize for searching a list of values
        public InElement(ExpressionElement operand, IList listElements)
        {
            MyOperand = operand;

            ExpressionElement[] arr = new ExpressionElement[listElements.Count];
            listElements.CopyTo(arr, 0);

            MyArguments = new List<ExpressionElement>(arr);
            this.ResolveForListSearch();
        }

        // Initialize for searching a collection
        public InElement(ExpressionElement operand, ExpressionElement targetCollection)
        {
            MyOperand = operand;
            MyTargetCollectionElement = targetCollection;
            this.ResolveForCollectionSearch();
        }

        private void ResolveForListSearch()
        {
            CompareElement ce = new CompareElement();

            // Validate that our operand is comparable to all elements in the list
            foreach (ExpressionElement argumentElement in MyArguments)
            {
                ce.Initialize(MyOperand, argumentElement, LogicalCompareOperation.Equal);
                ce.Validate();
            }
        }

        private void ResolveForCollectionSearch()
        {
            // Try to find a collection type
            MyTargetCollectionType = this.GetTargetCollectionType();

            if (MyTargetCollectionType == null)
            {
                base.ThrowCompileException(CompileErrorResourceKeys.SearchArgIsNotKnownCollectionType, CompileExceptionReason.TypeMismatch, MyTargetCollectionElement.ResultType.Name);
            }

            // Validate that the operand type is compatible with the collection
            MethodInfo mi = this.GetCollectionContainsMethod();
            ParameterInfo p1 = mi.GetParameters()[0];

            if (ImplicitConverter.EmitImplicitConvert(MyOperand.ResultType, p1.ParameterType, null) == false)
            {
                base.ThrowCompileException(CompileErrorResourceKeys.OperandNotConvertibleToCollectionType, CompileExceptionReason.TypeMismatch, MyOperand.ResultType.Name, p1.ParameterType.Name);
            }
        }

        private Type GetTargetCollectionType()
        {
            Type collType = MyTargetCollectionElement.ResultType;

            // Try to see if the collection is a generic ICollection or IDictionary
            Type[] interfaces = collType.GetInterfaces();

            foreach (Type interfaceType in interfaces)
            {
                if (interfaceType.IsGenericType == false)
                {
                    continue;
                }

                Type genericTypeDef = interfaceType.GetGenericTypeDefinition();

                if (object.ReferenceEquals(genericTypeDef, typeof(ICollection<>)) | object.ReferenceEquals(genericTypeDef, typeof(IDictionary<,>)))
                {
                    return interfaceType;
                }
            }

            // Try to see if it is a regular IList or IDictionary
            if (typeof(IList<>).IsAssignableFrom(collType) == true)
            {
                return typeof(IList<>);
            }
            else if (typeof(IDictionary<,>).IsAssignableFrom(collType) == true)
            {
                return typeof(IDictionary<,>);
            }

            // Not a known collection type
            return null;
        }

        public override void Emit(FleeILGenerator ilg, IServiceProvider services)
        {
            if ((MyTargetCollectionType != null))
            {
                this.EmitCollectionIn(ilg, services);

            }
            else
            {
                BranchManager bm = new BranchManager();
                bm.GetLabel("endLabel", ilg);
                bm.GetLabel("trueTerminal", ilg);

                // Do a fake emit to get branch positions
                FleeILGenerator ilgTemp = this.CreateTempFleeILGenerator(ilg);
                Utility.SyncFleeILGeneratorLabels(ilg, ilgTemp);

                this.EmitListIn(ilgTemp, services, bm);

                bm.ComputeBranches();

                // Do the real emit
                this.EmitListIn(ilg, services, bm);
            }
        }

        private void EmitCollectionIn(FleeILGenerator ilg, IServiceProvider services)
        {
            // Get the contains method
            MethodInfo mi = this.GetCollectionContainsMethod();
            ParameterInfo p1 = mi.GetParameters()[0];

            // Load the collection
            MyTargetCollectionElement.Emit(ilg, services);
            // Load the argument
            MyOperand.Emit(ilg, services);
            // Do an implicit convert if necessary
            ImplicitConverter.EmitImplicitConvert(MyOperand.ResultType, p1.ParameterType, ilg);
            // Call the contains method
            ilg.Emit(OpCodes.Callvirt, mi);
        }

        private MethodInfo GetCollectionContainsMethod()
        {
            string methodName = "Contains";

            if (MyTargetCollectionType.IsGenericType == true && object.ReferenceEquals(MyTargetCollectionType.GetGenericTypeDefinition(), typeof(IDictionary<,>)))
            {
                methodName = "ContainsKey";
            }

            return MyTargetCollectionType.GetMethod(methodName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
        }

        private void EmitListIn(FleeILGenerator ilg, IServiceProvider services, BranchManager bm)
        {
            CompareElement ce = new CompareElement();
            Label endLabel = bm.FindLabel("endLabel");
            Label trueTerminal = bm.FindLabel("trueTerminal");

            // Cache the operand since we will be comparing against it a lot
            LocalBuilder lb = ilg.DeclareLocal(MyOperand.ResultType);
            int targetIndex = lb.LocalIndex;

            MyOperand.Emit(ilg, services);
            Utility.EmitStoreLocal(ilg, targetIndex);

            // Wrap our operand in a local shim
            LocalBasedElement targetShim = new LocalBasedElement(MyOperand, targetIndex);

            // Emit the compares
            foreach (ExpressionElement argumentElement in MyArguments)
            {
                ce.Initialize(targetShim, argumentElement, LogicalCompareOperation.Equal);
                ce.Emit(ilg, services);

                EmitBranchToTrueTerminal(ilg, trueTerminal, bm);
            }

            ilg.Emit(OpCodes.Ldc_I4_0);
            ilg.Emit(OpCodes.Br_S, endLabel);

            bm.MarkLabel(ilg, trueTerminal);
            ilg.MarkLabel(trueTerminal);

            ilg.Emit(OpCodes.Ldc_I4_1);

            bm.MarkLabel(ilg, endLabel);
            ilg.MarkLabel(endLabel);
        }

        private static void EmitBranchToTrueTerminal(FleeILGenerator ilg, Label trueTerminal, BranchManager bm)
        {
            if (ilg.IsTemp == true)
            {
                bm.AddBranch(ilg, trueTerminal);
                ilg.Emit(OpCodes.Brtrue_S, trueTerminal);
            }
            else if (bm.IsLongBranch(ilg, trueTerminal) == false)
            {
                ilg.Emit(OpCodes.Brtrue_S, trueTerminal);
            }
            else
            {
                ilg.Emit(OpCodes.Brtrue, trueTerminal);
            }
        }

        public override System.Type ResultType => typeof(bool);
    }
}
