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
    internal class ConditionalElement : ExpressionElement
    {
        private readonly ExpressionElement _myCondition;
        private readonly ExpressionElement _myWhenTrue;
        private readonly ExpressionElement _myWhenFalse;
        private readonly Type _myResultType;
        public ConditionalElement(ExpressionElement condition, ExpressionElement whenTrue, ExpressionElement whenFalse)
        {
            _myCondition = condition;
            _myWhenTrue = whenTrue;
            _myWhenFalse = whenFalse;

            if ((!object.ReferenceEquals(_myCondition.ResultType, typeof(bool))))
            {
                base.ThrowCompileException(CompileErrorResourceKeys.FirstArgNotBoolean, CompileExceptionReason.TypeMismatch);
            }

            // The result type is the type that is common to the true/false operands
            if (ImplicitConverter.EmitImplicitConvert(_myWhenFalse.ResultType, _myWhenTrue.ResultType, null) == true)
            {
                _myResultType = _myWhenTrue.ResultType;
            }
            else if (ImplicitConverter.EmitImplicitConvert(_myWhenTrue.ResultType, _myWhenFalse.ResultType, null) == true)
            {
                _myResultType = _myWhenFalse.ResultType;
            }
            else
            {
                base.ThrowCompileException(CompileErrorResourceKeys.NeitherArgIsConvertibleToTheOther, CompileExceptionReason.TypeMismatch, _myWhenTrue.ResultType.Name, _myWhenFalse.ResultType.Name);
            }
        }

        public override void Emit(FleeILGenerator ilg, IServiceProvider services)
        {
            BranchManager bm = new BranchManager();
            bm.GetLabel("falseLabel", ilg);
            bm.GetLabel("endLabel", ilg);

            if (ilg.IsTemp == true)
            {
                // If this is a fake emit, then do a fake emit and return
                this.EmitConditional(ilg, services, bm);
                return;
            }

            FleeILGenerator ilgTemp = this.CreateTempFleeILGenerator(ilg);
            Utility.SyncFleeILGeneratorLabels(ilg, ilgTemp);

            // Emit fake conditional to get branch target positions
            this.EmitConditional(ilgTemp, services, bm);

            bm.ComputeBranches();

            // Emit real conditional now that we have the branch target locations
            this.EmitConditional(ilg, services, bm);
        }

        private void EmitConditional(FleeILGenerator ilg, IServiceProvider services, BranchManager bm)
        {
            Label falseLabel = bm.FindLabel("falseLabel");
            Label endLabel = bm.FindLabel("endLabel");

            // Emit the condition
            _myCondition.Emit(ilg, services);

            // On false go to the false operand
            if (ilg.IsTemp == true)
            {
                bm.AddBranch(ilg, falseLabel);
                ilg.Emit(OpCodes.Brfalse_S, falseLabel);
            }
            else if (bm.IsLongBranch(ilg, falseLabel) == false)
            {
                ilg.Emit(OpCodes.Brfalse_S, falseLabel);
            }
            else
            {
                ilg.Emit(OpCodes.Brfalse, falseLabel);
            }

            // Emit the true operand
            _myWhenTrue.Emit(ilg, services);
            ImplicitConverter.EmitImplicitConvert(_myWhenTrue.ResultType, _myResultType, ilg);

            // Jump to end
            if (ilg.IsTemp == true)
            {
                bm.AddBranch(ilg, endLabel);
                ilg.Emit(OpCodes.Br_S, endLabel);
            }
            else if (bm.IsLongBranch(ilg, endLabel) == false)
            {
                ilg.Emit(OpCodes.Br_S, endLabel);
            }
            else
            {
                ilg.Emit(OpCodes.Br, endLabel);
            }

            bm.MarkLabel(ilg, falseLabel);
            ilg.MarkLabel(falseLabel);

            // Emit the false operand
            _myWhenFalse.Emit(ilg, services);
            ImplicitConverter.EmitImplicitConvert(_myWhenFalse.ResultType, _myResultType, ilg);
            // Fall through to end
            bm.MarkLabel(ilg, endLabel);
            ilg.MarkLabel(endLabel);
        }

        public override System.Type ResultType => _myResultType;
    }
}
