using Flee.CalcEngine.PublicTypes;
using Flee.PublicTypes;
using NUnit.Framework;
using System.Collections.Generic;

namespace Flee.Test.CalcEngineTests
{
    [TestFixture]
    public class CalcEngineTestFixture
    {
        [Test]
        public void Test_Basic()
        {
            var ce = new CalculationEngine();
            var context = new ExpressionContext();
            var variables = context.Variables;

            variables.Add("x", 100);
            ce.Add("a", "x * 2", context);
            variables.Add("y", 1);
            ce.Add("b", "a + y", context);
            ce.Add("c", "b * 2", context);
            ce.Recalculate("a");

            var result = ce.GetResult<int>("c");
            Assert.AreEqual(result, ((100 * 2) + 1) * 2);
            variables.Remove("x");
            variables.Add("x", 345);
            ce.Recalculate("a");
            result = ce.GetResult<int>("c");

            Assert.AreEqual(((345 * 2) + 1) * 2, result);
        }

        [Test]
        public void Test_MutipleIdentical_References()
        {
            var ce = new CalculationEngine();
            var context = new ExpressionContext();
            var variables = context.Variables;

            variables.Add("x", 100);
            ce.Add("a", "x * 2", context);
            ce.Add("b", "a + a + a", context);
            ce.Recalculate("a");
            var result = ce.GetResult<int>("b");
            Assert.AreEqual((100 * 2) * 3, result);
        }

        [Test]
        public void Test_Complex()
        {
            var ce = new CalculationEngine();
            var context = new ExpressionContext();
            var variables = context.Variables;

            variables.Add("x", 100);
            ce.Add("a", "x * 2", context);
            variables.Add("y", 24);
            ce.Add("b", "y * 2", context);
            ce.Add("c", "a + b", context);
            ce.Add("d", "80", context);
            ce.Add("e", "a + b + c + d", context);
            ce.Recalculate("d");
            ce.Recalculate("a", "b");

            var result = ce.GetResult<int>("e");
            Assert.AreEqual((100 * 2) + (24 * 2) + ((100 * 2) + (24 * 2)) + 80, result);
        }

        [Test]
        public void Test_Arithmetic()
        {
            var ce = new CalculationEngine();
            var context = new ExpressionContext();
            var variables = context.Variables;

            variables.Add("a", 10);
            variables.Add("b", 20);
            ce.Add("x", "((a * 2) + (b ^ 2)) - (100 % 5)", context);
            ce.Recalculate("x");
            var result = ce.GetResult<int>("x");
            Assert.AreEqual(420, result);
        }

        [Test]
        public void Test_Comparison_Operators()
        {
            var ce = new CalculationEngine();
            var context = new ExpressionContext();
            var variables = context.Variables;

            variables.Add("a", 10);
            ce.Add("x", "a <> 100", context);
            ce.Recalculate("x");
            var result = ce.GetResult<bool>("x");
            Assert.IsTrue(result);
        }

        [Test]
        public void Test_And_Or_Xor_Not_Operators()
        {
            var ce = new CalculationEngine();
            var context = new ExpressionContext();
            var variables = context.Variables;

            variables.Add("a", 10);
            ce.Add("x", "a > 100", context);
            ce.Recalculate("x");
            var result = ce.GetResult<bool>("x");
            Assert.IsFalse(result);
            ce.Remove("x");
            variables.Add("b", 100);
            ce.Add("x", "b = 100", context);
            ce.Recalculate("x");
            result = ce.GetResult<bool>("x");
            Assert.IsTrue(result);
        }

        [Test]
        public void Test_Shift_Operators()
        {
            var ce = new CalculationEngine();
            var context = new ExpressionContext();
            var variables = context.Variables;

            ce.Add("x", "100 >> 2", context);
            ce.Recalculate("x");
            var result = ce.GetResult<int>("x");
            Assert.AreEqual(25, result);
        }

        [Test]
        public void Test_Recalculate_NonSource()
        {
            var ce = new CalculationEngine();
            var context = new ExpressionContext();
            var variables = context.Variables;

            variables.Add("x", 100);
            ce.Add("a", "x * 2", context);
            variables.Add("y", 1);
            ce.Add("b", "a + y", context);
            ce.Add("c", "b * 2", context);
            ce.Recalculate("a", "b");
            var result = ce.GetResult<int>("c");
            Assert.AreEqual(((100) * 2 + 1) * 2, result);
        }

        [Test]
        public void Test_Partial_Recalculate()
        {
            var ce = new CalculationEngine();
            var context = new ExpressionContext();
            var variables = context.Variables;

            variables.Add("x", 100);
            ce.Add("a", "x * 2", context);
            variables.Add("y", 1);
            ce.Add("b", "a + y", context);
            ce.Add("c", "b * 2", context);
            ce.Recalculate("a");
            variables["y"] = 222;
            ce.Recalculate("b");
            var result = ce.GetResult<int>("c");
            Assert.AreEqual(((100 * 2) + 222) * 2, result);
        }

        [Test]
        public void Test_Circular_Reference1()
        {
            var ce = new CalculationEngine();
            var context = new ExpressionContext();
            var variables = context.Variables;

            variables.Add("x", 100);
            ce.Add("a", "x * 2", context);
            variables.Add("y", 1);
            ce.Add("b", "a + y + b", context);
            Assert.Throws<CircularReferenceException>(() => { ce.Recalculate("a"); });
        }

        [Test]
        public void Test_Boolean_Expression()
        {
            string expression = "a AND NOT b AND NOT c AND d";
            Dictionary<string, object> expressionVariables = new Dictionary<string, object>();
            expressionVariables.Add("a", 1);
            expressionVariables.Add("b", 0);
            expressionVariables.Add("c", 0);
            expressionVariables.Add("d", 1);

            var context = new ExpressionContext();
            var vars = context.Variables;
            foreach (var expressionVariable in expressionVariables.Keys)
                vars.Add(expressionVariable, expressionVariables[expressionVariable]);
            IDynamicExpression dynamicExpression = context.CompileDynamic(expression);
            foreach (var expressionVariable in expressionVariables.Keys)
                vars[expressionVariable] = expressionVariables[expressionVariable];
            var a = dynamicExpression.Evaluate();

            //ExpressionContext context = new ExpressionContext();
            //VariableCollection variables = context.Variables;
            ////variables.Add("a", 1);
            ////variables.Add("b", 0);
            ////IGenericExpression<bool> e = context.CompileGeneric<bool>("a=1 OR b=0");

            //IGenericExpression<bool> e = context.CompileGeneric<bool>("false OR false");

            //bool result = e.Evaluate();
            //Assert.AreEqual(false, result);
        }
    }
}