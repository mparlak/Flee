using Flee.PublicTypes;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flee.Test.ExpressionTests
{
    public class Base
    {
        public double Value { get; set; }
        public static Base operator +(Base left, Base right)
        {
            return new Base { Value = left.Value + right.Value };
        }
        public static Base operator -(Base left)
        {
            return new Base { Value = -left.Value };
        }
    }

    public class Derived : Base
    {
    }

    public class OtherDerived : Base
    {
    }

    [TestFixture]
    public class CustomOperators
    {
        [Test]
        public void LeftBaseRightBase()
        {
            var m1 = new Base { Value = 2 };
            var m2 = new Base { Value = 5 };

            ExpressionContext context = new ExpressionContext();
            context.Variables.Add("m1", m1);
            context.Variables.Add("m2", m2);
            IDynamicExpression e1 = context.CompileDynamic("m1 + m2");

            Base added = (Base) e1.Evaluate();
            Assert.AreEqual(7, added.Value);
        }

        [Test]
        public void LeftBaseRightDerived()
        {
            var m1 = new Base { Value = 2 };
            var m2 = new Derived { Value = 5 };

            ExpressionContext context = new ExpressionContext();
            context.Variables.Add("m1", m1);
            context.Variables.Add("m2", m2);
            IDynamicExpression e1 = context.CompileDynamic("m1 + m2");

            Base added = (Base)e1.Evaluate();
            Assert.AreEqual(7, added.Value);
        }

        [Test]
        public void LeftDerivedRightBase()
        {
            var m1 = new Derived { Value = 2 };
            var m2 = new Base { Value = 5 };

            ExpressionContext context = new ExpressionContext();
            context.Variables.Add("m1", m1);
            context.Variables.Add("m2", m2);
            IDynamicExpression e1 = context.CompileDynamic("m1 + m2");

            Base added = (Base)e1.Evaluate();
            Assert.AreEqual(7, added.Value);
        }

        [Test]
        public void LeftDerivedRightDerived()
        {
            var m1 = new Derived { Value = 2 };
            var m2 = new Derived { Value = 5 };

            ExpressionContext context = new ExpressionContext();
            context.Variables.Add("m1", m1);
            context.Variables.Add("m2", m2);
            IDynamicExpression e1 = context.CompileDynamic("m1 + m2");

            Base added = (Base)e1.Evaluate();
            Assert.AreEqual(7, added.Value);
        }

        [Test]
        public void LeftDerivedRightOtherDerived()
        {
            var m1 = new Derived { Value = 2 };
            var m2 = new OtherDerived { Value = 5 };

            ExpressionContext context = new ExpressionContext();
            context.Variables.Add("m1", m1);
            context.Variables.Add("m2", m2);
            IDynamicExpression e1 = context.CompileDynamic("m1 + m2");

            Base added = (Base)e1.Evaluate();
            Assert.AreEqual(7, added.Value);
        }

        [Test]
        public void MissingOperator()
        {
            var m1 = new Derived { Value = 2 };
            var m2 = new OtherDerived { Value = 5 };

            ExpressionContext context = new ExpressionContext();
            context.Variables.Add("m1", m1);
            context.Variables.Add("m2", m2);

            var message = "ArithmeticElement: Operation 'Subtract' is not defined for types 'Derived' and 'OtherDerived'";
            Assert.Throws<ExpressionCompileException>(() => context.CompileDynamic("m1 - m2"), message);
        }

        [Test]
        public void BaseUnaryOperator()
        {
            var m1 = new Base { Value = 2 };

            ExpressionContext context = new ExpressionContext();
            context.Variables.Add("m1", m1);
            IDynamicExpression e1 = context.CompileDynamic("-m1");

            Base negated = (Base)e1.Evaluate();
            Assert.AreEqual(-2, negated.Value);
        }

        [Test]
        public void DerivedUnaryOperator()
        {
            var m1 = new Derived { Value = 2 };

            ExpressionContext context = new ExpressionContext();
            context.Variables.Add("m1", m1);
            IDynamicExpression e1 = context.CompileDynamic("-m1");

            Base negated = (Base)e1.Evaluate();
            Assert.AreEqual(-2, negated.Value);
        }

        [Test]
        public void DerivedUnaryOperatorPlusOperator()
        {
            var m1 = new Derived { Value = 2 };

            ExpressionContext context = new ExpressionContext();
            context.Variables.Add("m1", m1);
            IDynamicExpression e1 = context.CompileDynamic("-m1 + m1");

            Base negated = (Base)e1.Evaluate();
            Assert.AreEqual(0, negated.Value);
        }
    }
}
