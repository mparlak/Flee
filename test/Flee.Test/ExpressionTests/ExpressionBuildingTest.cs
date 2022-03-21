using System;
using Flee.PublicTypes;
using NUnit.Framework;

namespace ExpressionBuildingTest
{
    [TestFixture]
    public class ExpressionBuildingTest
    {
        [Test]
        public void ExpressionsAsVariables()
        {
            ExpressionContext context = new ExpressionContext();
            context.Imports.AddType(typeof(Math));
            context.Variables.Add("a", 3.14);
            IDynamicExpression e1 = context.CompileDynamic("cos(a) ^ 2");

            context = new ExpressionContext();
            context.Imports.AddType(typeof(Math));
            context.Variables.Add("a", 3.14);

            IDynamicExpression e2 = context.CompileDynamic("sin(a) ^ 2");

            // Use the two expressions as variables in another expression
            context = new ExpressionContext();
            context.Variables.Add("a", e1);
            context.Variables.Add("b", e2);
            IDynamicExpression e = context.CompileDynamic("a + b");

            Console.WriteLine(e.Evaluate());
        }


        [Test]
        public void Test_IfExpression_enUS()
        {
            ExpressionContext context = new ExpressionContext();
            context.Options.ParseCulture = new System.Globalization.CultureInfo("en-US");

            int resultWhenTrue = 3;

            IDynamicExpression e = context.CompileDynamic("if(1<2, 3, 4)");

            Assert.IsTrue((int)e.Evaluate() == resultWhenTrue);
        }

        [Test]
        public void Test_IfExpression_fiFI()
        {
            ExpressionContext context = new ExpressionContext();
            context.Imports.AddType(typeof(Math));
            context.Options.ParseCulture = new System.Globalization.CultureInfo("fi-FI");

            int resultWhenFalse = 4;

            IDynamicExpression e = context.CompileDynamic("if(1>2; 3; 4)");

            Assert.IsTrue((int)e.Evaluate() == resultWhenFalse);
        }

        [Test]
        public void NullCheck()
        {
            ExpressionContext context = new ExpressionContext();
            context.Variables.Add("a", "stringObject");
            IDynamicExpression e1 = context.CompileDynamic("a = null");

            Assert.IsFalse((bool)e1.Evaluate());
        }

        [Test]
        public void NullIsNullCheck()
        {
            ExpressionContext context = new ExpressionContext();
            context.Variables.Add("a", "stringObject");
            IDynamicExpression e1 = context.CompileDynamic("null = null");

            Assert.IsTrue((bool)e1.Evaluate());
        }

        [Test]
        public void CompareLongs()
        {
            // bug #83 test.
            ExpressionContext context = new ExpressionContext();
            IDynamicExpression e1 = context.CompileDynamic("2432696330L = 2432696330L AND 2432696330L > 0 AND 2432696330L < 2432696331L");

            Assert.IsTrue((bool)e1.Evaluate());
            e1 = context.CompileDynamic("2432696330L / 2");

            Assert.AreEqual(1216348165L, e1.Evaluate());
        }

        [Test]
        public void ArgumentInt_to_DoubleConversion()
        {
            ExpressionContext context = new ExpressionContext();
            context.Imports.AddType(typeof(Math));
            IDynamicExpression e1 = context.CompileDynamic("sqrt(16)");

            Assert.AreEqual(4.0, e1.Evaluate());
        }


        [Test]
        public void IN_OperatorTest()
        {
            ExpressionContext context = new ExpressionContext();
            var e1 = context.CompileGeneric<bool>("NOT 15 IN (1,2,3,4,5,6,7,8,9,10,11,12,13,14,16,17,18,19,20,21,22,23)");

            Assert.IsTrue(e1.Evaluate());

            e1 = context.CompileGeneric<bool>("\"a\" IN (\"a\",\"b\",\"c\",\"d\") and true and 5 in (2,4,5)");
            Assert.IsTrue(e1.Evaluate());
            e1 = context.CompileGeneric<bool>("\"a\" IN (\"a\",\"b\",\"c\",\"d\") and true and 5 in (2,4,6,7,8,9)");
            Assert.IsFalse(e1.Evaluate());
        }
    }
}