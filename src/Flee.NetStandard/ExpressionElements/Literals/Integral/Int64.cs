using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Reflection.Emit;
using Flee.ExpressionElements.Base.Literals;

using Flee.InternalTypes;

namespace Flee.ExpressionElements.Literals.Integral
{
    internal class Int64LiteralElement : IntegralLiteralElement
    {

        private Int64 _myValue;
        private const string MinValue = "9223372036854775808";

        private readonly bool _myIsMinValue;
        public Int64LiteralElement(Int64 value)
        {
            _myValue = value;
        }

        private Int64LiteralElement()
        {
            _myIsMinValue = true;
        }

        public static Int64LiteralElement TryCreate(string image, bool isHex, bool negated)
        {
            if (negated == true & image == MinValue)
            {
                return new Int64LiteralElement();
            }
            else if (isHex == true)
            {
                Int64 value = default(Int64);

                if (Int64.TryParse(image, NumberStyles.AllowHexSpecifier, null, out value) == false)
                {
                    return null;
                }
                else if (value >= 0 & value <= Int64.MaxValue)
                {
                    return new Int64LiteralElement(value);
                }
                else
                {
                    return null;
                }
            }
            else
            {
                Int64 value = default(Int64);

                if (Int64.TryParse(image, out value) == true)
                {
                    return new Int64LiteralElement(value);
                }
                else
                {
                    return null;
                }
            }
        }

        public override void Emit(FleeILGenerator ilg, IServiceProvider services)
        {
            EmitLoad(_myValue, ilg);
        }

        public void Negate()
        {
            if (_myIsMinValue == true)
            {
                _myValue = Int64.MinValue;
            }
            else
            {
                _myValue = -_myValue;
            }
        }

        public override System.Type ResultType => typeof(Int64);
    }
}
