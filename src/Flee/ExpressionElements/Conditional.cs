using System.Reflection.Emit;
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
            this.EmitConditional(ilg, services);
        }

        private void EmitConditional(FleeILGenerator ilg, IServiceProvider services)
        {
            Label falseLabel = ilg.DefineLabel();
            Label endLabel = ilg.DefineLabel();

            // Emit the condition
            _myCondition.Emit(ilg, services);

            // On false go to the false operand
            ilg.EmitBranchFalse(falseLabel);

            // Emit the true operand
            _myWhenTrue.Emit(ilg, services);
            ImplicitConverter.EmitImplicitConvert(_myWhenTrue.ResultType, _myResultType, ilg);

            // Jump to end
            ilg.EmitBranch(endLabel);

            ilg.MarkLabel(falseLabel);

            // Emit the false operand
            _myWhenFalse.Emit(ilg, services);
            ImplicitConverter.EmitImplicitConvert(_myWhenFalse.ResultType, _myResultType, ilg);
            // Fall through to end
            ilg.MarkLabel(endLabel);
        }

        public override System.Type ResultType => _myResultType;
    }
}
