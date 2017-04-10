using System;
using System.Globalization;
using System.Reflection.Emit;
using Flee.ExpressionElements.Base.Literals;

using Flee.InternalTypes;


namespace Flee.ExpressionElements.Literals.Integral
{
    internal class Int32LiteralElement : IntegralLiteralElement
    {
        private Int32 _myValue;
        private const string MinValue = "2147483648";
        private readonly bool _myIsMinValue;
        public Int32LiteralElement(Int32 value)
        {
            _myValue = value;
        }

        private Int32LiteralElement()
        {
            _myIsMinValue = true;
        }

        public static Int32LiteralElement TryCreate(string image, bool isHex, bool negated)
        {
            if (negated == true & image == MinValue)
            {
                return new Int32LiteralElement();
            }
            else if (isHex == true)
            {
                Int32 value = default(Int32);

                // Since Int32.TryParse will succeed for a string like 0xFFFFFFFF we have to do some special handling
                if (Int32.TryParse(image, NumberStyles.AllowHexSpecifier, null, out value) == false)
                {
                    return null;
                }
                else if (value >= 0 & value <= Int32.MaxValue)
                {
                    return new Int32LiteralElement(value);
                }
                else
                {
                    return null;
                }
            }
            else
            {
                Int32 value = default(Int32);

                if (Int32.TryParse(image,out value) == true)
                {
                    return new Int32LiteralElement(value);
                }
                else
                {
                    return null;
                }
            }
        }

        public void Negate()
        {
            if (_myIsMinValue == true)
            {
                _myValue = Int32.MinValue;
            }
            else
            {
                _myValue = -_myValue;
            }
        }

        public override void Emit(FleeILGenerator ilg, IServiceProvider services)
        {
            EmitLoad(_myValue, ilg);
        }

        public override System.Type ResultType => typeof(Int32);

        public int Value => _myValue;
    }
}
