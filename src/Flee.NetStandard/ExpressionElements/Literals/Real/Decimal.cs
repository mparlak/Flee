using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection.Emit;
using System.Reflection;
using Flee.ExpressionElements.Base.Literals;
using Flee.InternalTypes;
using Flee.PublicTypes;


namespace Flee.ExpressionElements.Literals.Real
{
    internal class DecimalLiteralElement : RealLiteralElement
    {
        private static readonly ConstructorInfo OurConstructorInfo = GetConstructor();
        private readonly decimal _myValue;

        private DecimalLiteralElement()
        {
        }

        public DecimalLiteralElement(decimal value)
        {
            _myValue = value;
        }

        private static ConstructorInfo GetConstructor()
        {
            Type[] types = {
            typeof(Int32),
            typeof(Int32),
            typeof(Int32),
            typeof(bool),
            typeof(byte)
        };
            return typeof(decimal).GetConstructor(BindingFlags.Instance | BindingFlags.Public, null, CallingConventions.Any, types, null);
        }

        public static DecimalLiteralElement Parse(string image, IServiceProvider services)
        {
            ExpressionParserOptions options = (ExpressionParserOptions)services.GetService(typeof(ExpressionParserOptions));
            DecimalLiteralElement element = new DecimalLiteralElement();

            try
            {
                decimal value = options.ParseDecimal(image);
                return new DecimalLiteralElement(value);
            }
            catch (OverflowException ex)
            {
                element.OnParseOverflow(image);
                return null;
            }
        }

        public override void Emit(FleeILGenerator ilg, IServiceProvider services)
        {
            int index = ilg.GetTempLocalIndex(typeof(decimal));
            Utility.EmitLoadLocalAddress(ilg, index);

            int[] bits = decimal.GetBits(_myValue);
            EmitLoad(bits[0], ilg);
            EmitLoad(bits[1], ilg);
            EmitLoad(bits[2], ilg);

            int flags = bits[3];

            EmitLoad((flags >> 31) == -1, ilg);

            EmitLoad(flags >> 16, ilg);

            ilg.Emit(OpCodes.Call, OurConstructorInfo);

            Utility.EmitLoadLocal(ilg, index);
        }

        public override System.Type ResultType => typeof(decimal);
    }
}
