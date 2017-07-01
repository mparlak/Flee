using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using Flee.ExpressionElements.Literals.Real;
using Flee.PublicTypes;

namespace Flee.ExpressionElements.Base.Literals
{
    internal abstract class RealLiteralElement : LiteralElement
    {
        protected RealLiteralElement()
        {
        }

        public static LiteralElement CreateFromInteger(string image, IServiceProvider services)
        {
            LiteralElement element = default(LiteralElement);

            element = CreateSingle(image, services);

            if ((element != null))
            {
                return element;
            }

            element = CreateDecimal(image, services);

            if ((element != null))
            {
                return element;
            }

            ExpressionOptions options = (ExpressionOptions)services.GetService(typeof(ExpressionOptions));

            // Convert to a double if option is set
            if (options.IntegersAsDoubles == true)
            {
                return DoubleLiteralElement.Parse(image, services);
            }

            return null;
        }

        public static LiteralElement Create(string image, IServiceProvider services)
        {
            LiteralElement element = default(LiteralElement);

            element = CreateSingle(image, services);

            if ((element != null))
            {
                return element;
            }

            element = CreateDecimal(image, services);

            if ((element != null))
            {
                return element;
            }

            element = CreateDouble(image, services);

            if ((element != null))
            {
                return element;
            }

            element = CreateImplicitReal(image, services);

            return element;
        }

        private static LiteralElement CreateImplicitReal(string image, IServiceProvider services)
        {
            ExpressionOptions options = (ExpressionOptions)services.GetService(typeof(ExpressionOptions));
            RealLiteralDataType realType = options.RealLiteralDataType;

            switch (realType)
            {
                case RealLiteralDataType.Double:
                    return DoubleLiteralElement.Parse(image, services);
                case RealLiteralDataType.Single:
                    return SingleLiteralElement.Parse(image, services);
                case RealLiteralDataType.Decimal:
                    return DecimalLiteralElement.Parse(image, services);
                default:
                    Debug.Fail("Unknown value");
                    return null;
            }
        }

        private static DoubleLiteralElement CreateDouble(string image, IServiceProvider services)
        {
            if (image.EndsWith("d", StringComparison.OrdinalIgnoreCase) == true)
            {
                image = image.Remove(image.Length - 1);
                return DoubleLiteralElement.Parse(image, services);
            }
            else
            {
                return null;
            }
        }

        private static SingleLiteralElement CreateSingle(string image, IServiceProvider services)
        {
            if (image.EndsWith("f", StringComparison.OrdinalIgnoreCase) == true)
            {
                image = image.Remove(image.Length - 1);
                return SingleLiteralElement.Parse(image, services);
            }
            else
            {
                return null;
            }
        }

        private static DecimalLiteralElement CreateDecimal(string image, IServiceProvider services)
        {
            if (image.EndsWith("m", StringComparison.OrdinalIgnoreCase) == true)
            {
                image = image.Remove(image.Length - 1);
                return DecimalLiteralElement.Parse(image, services);
            }
            else
            {
                return null;
            }
        }
    }
}
