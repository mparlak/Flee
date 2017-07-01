using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection.Emit;
using Flee.ExpressionElements.Base.Literals;
using Flee.InternalTypes;

namespace Flee.ExpressionElements.Literals.Integral
{
    internal class UInt32LiteralElement : IntegralLiteralElement
    {
        private readonly UInt32 _myValue;
        public UInt32LiteralElement(UInt32 value)
        {
            _myValue = value;
        }

        public static UInt32LiteralElement TryCreate(string image, System.Globalization.NumberStyles ns)
        {
            UInt32 value = default(UInt32);
            if (UInt32.TryParse(image, ns, null, out value) == true)
            {
                return new UInt32LiteralElement(value);
            }
            else
            {
                return null;
            }
        }

        public override void Emit(FleeILGenerator ilg, IServiceProvider services)
        {
            EmitLoad(Convert.ToInt32(_myValue), ilg);
        }

        public override System.Type ResultType => typeof(UInt32);
    }
}
