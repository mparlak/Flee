using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Flee.PublicTypes;

namespace ExpressionBuildingTest
{
    [TestClass]
    public class ExpressionBuildingTest
    {
        [TestMethod]
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


        [TestMethod]
        public void Test_IfExpression_enUS()
        {
            ExpressionContext context = new ExpressionContext();
            context.Options.ParseCulture = new System.Globalization.CultureInfo("en-US");

            int resultWhenTrue = 3;

            IDynamicExpression e = context.CompileDynamic("if(1<2, 3, 4)");

            Assert.IsTrue((int)e.Evaluate() == resultWhenTrue);
        }

        [TestMethod]
        public void Test_IfExpression_fiFI()
        {
            ExpressionContext context = new ExpressionContext();
            context.Imports.AddType(typeof(Math));
            context.Options.ParseCulture = new System.Globalization.CultureInfo("fi-FI");

            int resultWhenFalse = 4;

            IDynamicExpression e = context.CompileDynamic("if(1>2; 3; 4)");

            Assert.IsTrue((int)e.Evaluate() == resultWhenFalse);
        }

        [TestMethod]
        public void NullCheck()
        {
            ExpressionContext context = new ExpressionContext();
            context.Variables.Add("a", "stringObject");
            IDynamicExpression e1 = context.CompileDynamic("a = null");

            Assert.IsFalse((bool)e1.Evaluate());
        }

        [TestMethod]
        public void NullIsNullCheck()
        {
            ExpressionContext context = new ExpressionContext();
            context.Variables.Add("a", "stringObject");
            IDynamicExpression e1 = context.CompileDynamic("null = null");

            Assert.IsTrue((bool)e1.Evaluate());
        }

        [TestMethod, ExpectedException(typeof(ExpressionEvaluationException))]
        public void RuntimeErrorCheck_DivisionByZero()
        {
            ExpressionContext context = new ExpressionContext();
            IDynamicExpression e1 = context.CompileDynamic("1 = 1/0");
            e1.Evaluate();
        }

        [TestMethod, ExpectedException(typeof(ExpressionEvaluationException))]
        public void RuntimeErrorCheck_ParsingInvalidDate()
        {
            ExpressionContext context = new ExpressionContext();
            context.Variables.Add("a", "stringObject");
            context.Imports.AddType(typeof(DateTime));
            IDynamicExpression e1 = context.CompileDynamic("Parse(a)");
            e1.Evaluate();
        }
    }
}