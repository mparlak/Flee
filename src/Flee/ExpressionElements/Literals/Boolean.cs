using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection.Emit;
using Flee.ExpressionElements.Base.Literals;

using Flee.InternalTypes;

namespace Flee.ExpressionElements.Literals
{
    internal class BooleanLiteralElement : LiteralElement
    {
        private readonly bool _myValue;
        public BooleanLiteralElement(bool value)
        {
            _myValue = value;
        }

        public override void Emit(FleeILGenerator ilg, IServiceProvider services)
        {
            EmitLoad(_myValue, ilg);
        }

        public override System.Type ResultType => typeof(bool);
    }
}
