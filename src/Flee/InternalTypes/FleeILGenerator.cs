using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Reflection.Emit;

namespace Flee.InternalTypes
{
    internal class FleeILGenerator
    {
        private readonly ILGenerator _myIlGenerator;
        private int _myLength;
        private int _myLabelCount;
        private readonly Dictionary<Type, LocalBuilder> _localBuilderTemp;
        private readonly bool _myIsTemp;
        public FleeILGenerator(ILGenerator ilg, int startLength = 0, bool isTemp = false)
        {
            _myIlGenerator = ilg;
            _localBuilderTemp = new Dictionary<Type, LocalBuilder>();
            _myIsTemp = isTemp;
            _myLength = startLength;
        }

        public int GetTempLocalIndex(Type localType)
        {
            LocalBuilder local = null;

            if (_localBuilderTemp.TryGetValue(localType, out local) == false)
            {
                local = _myIlGenerator.DeclareLocal(localType);
                _localBuilderTemp.Add(localType, local);
            }

            return local.LocalIndex;
        }

        public void Emit(OpCode op)
        {
            this.RecordOpcode(op);
            _myIlGenerator.Emit(op);
        }

        public void Emit(OpCode op, Type arg)
        {
            this.RecordOpcode(op);
            _myIlGenerator.Emit(op, arg);
        }

        public void Emit(OpCode op, ConstructorInfo arg)
        {
            this.RecordOpcode(op);
            _myIlGenerator.Emit(op, arg);
        }

        public void Emit(OpCode op, MethodInfo arg)
        {
            this.RecordOpcode(op);
            _myIlGenerator.Emit(op, arg);
        }

        public void Emit(OpCode op, FieldInfo arg)
        {
            this.RecordOpcode(op);
            _myIlGenerator.Emit(op, arg);
        }

        public void Emit(OpCode op, byte arg)
        {
            this.RecordOpcode(op);
            _myIlGenerator.Emit(op, arg);
        }

        public void Emit(OpCode op, sbyte arg)
        {
            this.RecordOpcode(op);
            _myIlGenerator.Emit(op, arg);
        }

        public void Emit(OpCode op, short arg)
        {
            this.RecordOpcode(op);
            _myIlGenerator.Emit(op, arg);
        }

        public void Emit(OpCode op, int arg)
        {
            this.RecordOpcode(op);
            _myIlGenerator.Emit(op, arg);
        }

        public void Emit(OpCode op, long arg)
        {
            this.RecordOpcode(op);
            _myIlGenerator.Emit(op, arg);
        }

        public void Emit(OpCode op, float arg)
        {
            this.RecordOpcode(op);
            _myIlGenerator.Emit(op, arg);
        }

        public void Emit(OpCode op, double arg)
        {
            this.RecordOpcode(op);
            _myIlGenerator.Emit(op, arg);
        }

        public void Emit(OpCode op, string arg)
        {
            this.RecordOpcode(op);
            _myIlGenerator.Emit(op, arg);
        }

        public void Emit(OpCode op, Label arg)
        {
            this.RecordOpcode(op);
            _myIlGenerator.Emit(op, arg);
        }

        public void MarkLabel(Label lbl)
        {
            _myIlGenerator.MarkLabel(lbl);
        }

        public Label DefineLabel()
        {
            _myLabelCount += 1;
            return _myIlGenerator.DefineLabel();
        }

        public LocalBuilder DeclareLocal(Type localType)
        {
            return _myIlGenerator.DeclareLocal(localType);
        }

        private void RecordOpcode(OpCode op)
        {
            //Trace.WriteLine(String.Format("{0:x}: {1}", MyLength, op.Name))
            int operandLength = GetOpcodeOperandSize(op.OperandType);
            _myLength += op.Size + operandLength;
        }

        private static int GetOpcodeOperandSize(OperandType operand)
        {
            switch (operand)
            {
                case OperandType.InlineNone:
                    return 0;
                case OperandType.ShortInlineBrTarget:
                case OperandType.ShortInlineI:
                case OperandType.ShortInlineVar:
                    return 1;
                case OperandType.InlineVar:
                    return 2;
                case OperandType.InlineBrTarget:
                case OperandType.InlineField:
                case OperandType.InlineI:
                case OperandType.InlineMethod:
                case OperandType.InlineSig:
                case OperandType.InlineString:
                case OperandType.InlineTok:
                case OperandType.InlineType:
                case OperandType.ShortInlineR:
                    return 4;
                case OperandType.InlineI8:
                case OperandType.InlineR:
                    return 8;
                default:
                    Debug.Fail("Unknown operand type");
                    break;
            }
            return 0;
        }

        [Conditional("DEBUG")]
        public void ValidateLength()
        {
            Debug.Assert(this.Length == this.ILGeneratorLength, "ILGenerator length mismatch");
        }

        public int Length => _myLength;

        public int LabelCount => _myLabelCount;

        private int ILGeneratorLength => Utility.GetILGeneratorLength(_myIlGenerator);

        public bool IsTemp => _myIsTemp;
    }
}
