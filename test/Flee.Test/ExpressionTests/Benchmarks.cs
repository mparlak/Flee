using System;
using System.Diagnostics;
using Flee.PublicTypes;
using NUnit.Framework;

namespace Flee.Test.ExpressionTests
{
    [TestFixture]
    public class Benchmarks : Core
    {
        [Test(Description = "Test that setting variables is fast")]
        public void TestFastVariables()
        {
            //Test should take 200ms or less
            const int expectedTime = 200;
            const int iterations = 100000;

            var context = new ExpressionContext();
            var vars = context.Variables;
            vars.DefineVariable("a", typeof(int));
            vars.DefineVariable("b", typeof(int));
            IDynamicExpression e = this.CreateDynamicExpression("a + b", context);

            var sw = new Stopwatch();
            sw.Start();

            for (int i = 0; i < iterations - 1; i++)
            {
                vars["a"] = 200;
                vars["b"] = 300;
                var result = e.Evaluate();
                
            }
            sw.Stop();
            this.PrintSpeedMessage("Fast variables", iterations, sw);
            Assert.Less(sw.ElapsedMilliseconds, expectedTime, "Test time above expected value");
        }

        private const String BigExpression = @"
( 
    NOT 
    (
        (VAR1 > 0 
        OR VAR2 > 0 
        OR VAR3 > 0) 
        AND 
        (
            ( 28 In (VAR4)) 
            OR ( 28 In (VAR5)) 
            OR ( 28 In (VAR6)) 
            OR ( 28 In (VAR7)) 
            OR ( 28 In (VAR8)) 
            OR ( 28 In (VAR9)) 
            OR ( 28 In (VAR10)) 
            OR ( 28 In (VAR11)) 
            OR ( 28 In (VAR12)) 
            OR (24 In (VAR4)) 
            OR ( 24 In (VAR5)) 
            OR ( 24 In (VAR6)) 
            OR ( 24 In (VAR7)) 
            OR ( 24 In (VAR8)) 
            OR ( 24 In (VAR9)) 
            OR ( 24 In (VAR10)) 
            OR ( 24 In (VAR11)) 
            OR ( 24 In (VAR12)) 
            OR (25 In (VAR4)) 
            OR ( 25 In (VAR5)) 
            OR ( 25 In (VAR6)) 
            OR ( 25 In (VAR7)) 
            OR ( 25 In (VAR8)) 
            OR ( 25 In (VAR9)) 
            OR ( 25 In (VAR10)) 
            OR ( 25 In (VAR11)) 
            OR ( 25 In (VAR12))
        ) 
        AND 
        (
	        ( NOT 44 In (VAR4)) 
            AND ( NOT 44 In (VAR5)) 
            AND ( NOT 44 In (VAR6)) 
            AND ( NOT 44 In (VAR7)) 
            AND ( NOT 44 In (VAR8)) 
            AND ( NOT 44 In (VAR9)) 
            AND ( NOT 44 In (VAR10)) 
            AND ( NOT 44 In (VAR11)) 
            AND ( NOT 44 In (VAR12)) 
            AND ( NOT 34 In (VAR4)) 
            AND ( NOT 34 In (VAR5)) 
            AND ( NOT 34 In (VAR6)) 
            AND ( NOT 34 In (VAR7)) 
            AND ( NOT 34 In (VAR8)) 
            AND ( NOT 34 In (VAR9)) 
            AND ( NOT 34 In (VAR10)) 
            AND ( NOT 34 In (VAR11)) 
            AND ( NOT 34 In (VAR12)) 
            AND ( NOT 183 In (VAR4)) 
            AND ( NOT 183 In (VAR5)) 
            AND ( NOT 183 In (VAR6)) 
            AND ( NOT 183 In (VAR7)) 
            AND ( NOT 183 In (VAR8)) 
            AND ( NOT 183 In (VAR9)) 
            AND ( NOT 183 In (VAR10)) 
            AND ( NOT 183 In (VAR11)) 
            AND ( NOT 183 In (VAR12)) 
            AND ( NOT 81 In (VAR4)) 
            AND ( NOT 81 In (VAR5)) 
            AND ( NOT 81 In (VAR6)) 
            AND ( NOT 81 In (VAR7)) 
            AND ( NOT 81 In (VAR8)) 
            AND ( NOT 81 In (VAR9)) 
            AND ( NOT 81 In (VAR10)) 
            AND ( NOT 81 In (VAR11)) 
            AND ( NOT 81 In (VAR12))
        )
    )
) 
AND 
( 
    NOT 
    (
        (VAR1 > 0 
        OR VAR2 > 0 
        OR VAR3 > 0) 
        AND 
        (
            ( NOT 28 In (VAR4)) 
            AND ( NOT 28 In (VAR5)) 
            AND ( NOT 28 In (VAR6)) 
            AND ( NOT 28 In (VAR7)) 
            AND ( NOT 28 In (VAR8)) 
            AND ( NOT 28 In (VAR9)) 
            AND ( NOT 28 In (VAR10)) 
            AND ( NOT 28 In (VAR11)) 
            AND ( NOT 28 In (VAR12)) 
            AND ( NOT 24 In (VAR4)) 
            AND ( NOT 24 In (VAR5)) 
            AND ( NOT 24 In (VAR6)) 
            AND ( NOT 24 In (VAR7)) 
            AND ( NOT 24 In (VAR8)) 
            AND ( NOT 24 In (VAR9)) 
            AND ( NOT 24 In (VAR10)) 
            AND ( NOT 24 In (VAR11)) 
            AND ( NOT 24 In (VAR12)) 
            AND ( NOT 25 In (VAR4)) 
            AND ( NOT 25 In (VAR5)) 
            AND ( NOT 25 In (VAR6)) 
            AND ( NOT 25 In (VAR7)) 
            AND ( NOT 25 In (VAR8)) 
            AND ( NOT 25 In (VAR9)) 
            AND ( NOT 25 In (VAR10)) 
            AND ( NOT 25 In (VAR11)) 
            AND ( NOT 25 In (VAR12))
        ) 
        AND 
        (
            ( NOT 44 In (VAR4)) 
            AND ( NOT 44 In (VAR5)) 
            AND ( NOT 44 In (VAR6)) 
            AND ( NOT 44 In (VAR7)) 
            AND ( NOT 44 In (VAR8)) 
            AND ( NOT 44 In (VAR9)) 
            AND ( NOT 44 In (VAR10)) 
            AND ( NOT 44 In (VAR11)) 
            AND ( NOT 44 In (VAR12)) 
            AND ( NOT 34 In (VAR4)) 
            AND ( NOT 34 In (VAR5)) 
            AND ( NOT 34 In (VAR6)) 
            AND ( NOT 34 In (VAR7)) 
            AND ( NOT 34 In (VAR8)) 
            AND ( NOT 34 In (VAR9)) 
            AND ( NOT 34 In (VAR10)) 
            AND ( NOT 34 In (VAR11)) 
            AND ( NOT 34 In (VAR12)) 
            AND ( NOT 183 In (VAR4)) 
            AND ( NOT 183 In (VAR5)) 
            AND ( NOT 183 In (VAR6)) 
            AND ( NOT 183 In (VAR7)) 
            AND ( NOT 183 In (VAR8)) 
            AND ( NOT 183 In (VAR9)) 
            AND ( NOT 183 In (VAR10)) 
            AND ( NOT 183 In (VAR11)) 
            AND ( NOT 183 In (VAR12)) 
            AND ( NOT 81 In (VAR4)) 
            AND ( NOT 81 In (VAR5)) 
            AND ( NOT 81 In (VAR6)) 
            AND ( NOT 81 In (VAR7)) 
            AND ( NOT 81 In (VAR8)) 
            AND ( NOT 81 In (VAR9)) 
            AND ( NOT 81 In (VAR10)) 
            AND ( NOT 81 In (VAR11)) 
            AND ( NOT 81 In (VAR12))
        )
    )
) 
AND 
(
    NOT 
    (
        (VAR1 > 0 
        OR VAR2 > 0 
        OR VAR3 > 0) 
        AND 
        (
            ( 28 In (VAR4)) 
            OR ( 28 In (VAR5)) 
            OR ( 28 In (VAR6)) 
            OR ( 28 In (VAR7)) 
            OR ( 28 In (VAR8)) 
            OR ( 28 In (VAR9)) 
            OR ( 28 In (VAR10)) 
            OR ( 28 In (VAR11)) 
            OR ( 28 In (VAR12)) 
            OR ( 24 In (VAR4)) 
            OR ( 24 In (VAR5)) 
            OR ( 24 In (VAR6)) 
            OR ( 24 In (VAR7)) 
            OR ( 24 In (VAR8)) 
            OR ( 24 In (VAR9)) 
            OR ( 24 In (VAR10)) 
            OR ( 24 In (VAR11)) 
            OR ( 24 In (VAR12)) 
            OR ( 25 In (VAR4)) 
            OR ( 25 In (VAR5)) 
            OR ( 25 In (VAR6))
            OR ( 25 In (VAR7)) 
            OR ( 25 In (VAR8)) 
            OR ( 25 In (VAR9)) 
            OR ( 25 In (VAR10)) 
            OR ( 25 In (VAR11)) 
            OR ( 25 In (VAR12))
        ) 
        AND 
        (
            ( 44 In (VAR4)) 
            OR ( 44 In (VAR5)) 
            OR ( 44 In (VAR6)) 
            OR ( 44 In (VAR7)) 
            OR ( 44 In (VAR8)) 
            OR ( 44 In (VAR9)) 
            OR ( 44 In (VAR10)) 
            OR ( 44 In (VAR11)) 
            OR ( 44 In (VAR12))
        )
    )
) 
AND 
( 
    NOT 
    (
        (VAR1 > 0 
        OR VAR2 > 0 
        OR VAR3 > 0) 
        AND 
        (
            ( NOT 28 In (VAR4)) 
            AND ( NOT 28 In (VAR5)) 
            AND ( NOT 28 In (VAR6)) 
            AND ( NOT 28 In (VAR7)) 
            AND ( NOT 28 In (VAR8)) 
            AND ( NOT 28 In (VAR9)) 
            AND ( NOT 28 In (VAR10)) 
            AND ( NOT 28 In (VAR11)) 
            AND ( NOT 28 In (VAR12)) 
            AND ( NOT 24 In (VAR4)) 
            AND ( NOT 24 In (VAR5)) 
            AND ( NOT 24 In (VAR6)) 
            AND ( NOT 24 In (VAR7)) 
            AND ( NOT 24 In (VAR8)) 
            AND ( NOT 24 In (VAR9)) 
            AND ( NOT 24 In (VAR10)) 
            AND ( NOT 24 In (VAR11)) 
            AND ( NOT 24 In (VAR12)) 
            AND ( NOT 25 In (VAR4)) 
            AND ( NOT 25 In (VAR5)) 
            AND ( NOT 25 In (VAR6)) 
            AND ( NOT 25 In (VAR7)) 
            AND ( NOT 25 In (VAR8)) 
            AND ( NOT 25 In (VAR9)) 
            AND ( NOT 25 In (VAR10)) 
            AND ( NOT 25 In (VAR11)) 
            AND ( NOT 25 In (VAR12))
        ) 
        AND 
        (
            ( 44 In (VAR4)) 
            OR ( 44 In (VAR5)) 
            OR ( 44 In (VAR6)) 
            OR ( 44 In (VAR7)) 
            OR ( 44 In (VAR8)) 
            OR ( 44 In (VAR9)) 
            OR ( 44 In (VAR10)) 
            OR ( 44 In (VAR11)) 
            OR ( 44 In (VAR12))
        )
    )
) 
AND 
( 
    NOT 
    (
        (VAR1 > 0 
        OR VAR2 > 0 
        OR VAR3 > 0) 
        AND 
        (
            ( 28 In (VAR4)) 
            OR ( 28 In (VAR5)) 
            OR ( 28 In (VAR6)) 
            OR ( 28 In (VAR7)) 
            OR ( 28 In (VAR8)) 
            OR ( 28 In (VAR9)) 
            OR ( 28 In (VAR10)) 
            OR ( 28 In (VAR11)) 
            OR ( 28 In (VAR12)) 
            OR ( 24 In (VAR4)) 
            OR ( 24 In (VAR5)) 
            OR ( 24 In (VAR6)) 
            OR ( 24 In (VAR7)) 
            OR ( 24 In (VAR8)) 
            OR ( 24 In (VAR9)) 
            OR ( 24 In (VAR10)) 
            OR ( 24 In (VAR11)) 
            OR ( 24 In (VAR12)) 
            OR ( 25 In (VAR4)) 
            OR ( 25 In (VAR5)) 
            OR ( 25 In (VAR6)) 
            OR ( 25 In (VAR7)) 
            OR ( 25 In (VAR8)) 
            OR ( 25 In (VAR9)) 
            OR ( 25 In (VAR10)) 
            OR ( 25 In (VAR11)) 
            OR ( 25 In (VAR12))
        ) 
        AND 
        (
            ( 34 In (VAR4)) 
            OR ( 34 In (VAR5)) 
            OR ( 34 In (VAR6)) 
            OR ( 34 In (VAR7)) 
            OR ( 34 In (VAR8)) 
            OR ( 34 In (VAR9)) 
            OR ( 34 In (VAR10)) 
            OR ( 34 In (VAR11)) 
            OR ( 34 In (VAR12))
        )
    )
) 
AND 
( 
    NOT 
    (
        (VAR1 > 0 
        OR VAR2 > 0 
        OR VAR3 > 0) 
        AND 
        (
            ( NOT 28 In (VAR4)) 
            AND ( NOT 28 In (VAR5)) 
            AND ( NOT 28 In (VAR6)) 
            AND ( NOT 28 In (VAR7)) 
            AND ( NOT 28 In (VAR8)) 
            AND ( NOT 28 In (VAR9)) 
            AND ( NOT 28 In (VAR10)) 
            AND ( NOT 28 In (VAR11)) 
            AND ( NOT 28 In (VAR12)) 
            AND ( NOT 24 In (VAR4)) 
            AND ( NOT 24 In (VAR5)) 
            AND ( NOT 24 In (VAR6)) 
            AND ( NOT 24 In (VAR7)) 
            AND ( NOT 24 In (VAR8)) 
            AND ( NOT 24 In (VAR9)) 
            AND ( NOT 24 In (VAR10)) 
            AND ( NOT 24 In (VAR11)) 
            AND ( NOT 24 In (VAR12)) 
            AND ( NOT 25 In (VAR4)) 
            AND ( NOT 25 In (VAR5)) 
            AND ( NOT 25 In (VAR6)) 
            AND ( NOT 25 In (VAR7)) 
            AND ( NOT 25 In (VAR8)) 
            AND ( NOT 25 In (VAR9)) 
            AND ( NOT 25 In (VAR10)) 
            AND ( NOT 25 In (VAR11)) 
            AND ( NOT 25 In (VAR12))
        ) 
        AND 
        (
            ( 34 In (VAR4)) 
            OR ( 34 In (VAR5)) 
            OR ( 34 In (VAR6)) 
            OR ( 34 In (VAR7)) 
            OR ( 34 In (VAR8)) 
            OR ( 34 In (VAR9)) 
            OR ( 34 In (VAR10)) 
            OR ( 34 In (VAR11)) 
            OR ( 34 In (VAR12))
        )
    )
) 
AND 
( 
    NOT 
    (
        (VAR1 > 0 
        OR VAR2 > 0 
        OR VAR3 > 0) 
        AND 
        (
            ( 28 In (VAR4)) 
            OR ( 28 In (VAR5)) 
            OR ( 28 In (VAR6)) 
            OR ( 28 In (VAR7)) 
            OR ( 28 In (VAR8)) 
            OR ( 28 In (VAR9)) 
            OR ( 28 In (VAR10)) 
            OR ( 28 In (VAR11)) 
            OR ( 28 In (VAR12)) 
            OR ( 24 In (VAR4)) 
            OR ( 24 In (VAR5)) 
            OR ( 24 In (VAR6)) 
            OR ( 24 In (VAR7)) 
            OR ( 24 In (VAR8)) 
            OR ( 24 In (VAR9)) 
            OR ( 24 In (VAR10)) 
            OR ( 24 In (VAR11)) 
            OR ( 24 In (VAR12)) 
            OR ( 25 In (VAR4)) 
            OR ( 25 In (VAR5)) 
            OR ( 25 In (VAR6)) 
            OR ( 25 In (VAR7)) 
            OR ( 25 In (VAR8)) 
            OR ( 25 In (VAR9)) 
            OR ( 25 In (VAR10)) 
            OR ( 25 In (VAR11)) 
            OR ( 25 In (VAR12))
        ) 
        AND 
        (
            ( 81 In (VAR4)) 
            OR ( 81 In (VAR5)) 
            OR ( 81 In (VAR6)) 
            OR ( 81 In (VAR7)) 
            OR ( 81 In (VAR8)) 
            OR ( 81 In (VAR9)) 
            OR ( 81 In (VAR10)) 
            OR ( 81 In (VAR11)) 
            OR ( 81 In (VAR12)) 
            OR ( 183 In (VAR4)) 
            OR ( 183 In (VAR5)) 
            OR ( 183 In (VAR6)) 
            OR ( 183 In (VAR7)) 
            OR ( 183 In (VAR8)) 
            OR ( 183 In (VAR9)) 
            OR ( 183 In (VAR10)) 
            OR ( 183 In (VAR11)) 
            OR ( 183 In (VAR12))
        )
    )
) 
AND 
( 
    NOT 
    (
        (VAR1 > 0 
        OR VAR2 > 0 
        OR VAR3 > 0) 
        AND 
        (
            ( NOT 28 In (VAR4)) 
            AND ( NOT 28 In (VAR5)) 
            AND ( NOT 28 In (VAR6)) 
            AND ( NOT 28 In (VAR7)) 
            AND ( NOT 28 In (VAR8)) 
            AND ( NOT 28 In (VAR9)) 
            AND ( NOT 28 In (VAR10)) 
            AND ( NOT 28 In (VAR11)) 
            AND ( NOT 28 In (VAR12)) 
            AND ( NOT 24 In (VAR4)) 
            AND ( NOT 24 In (VAR5)) 
            AND ( NOT 24 In (VAR6)) 
            AND ( NOT 24 In (VAR7)) 
            AND ( NOT 24 In (VAR8)) 
            AND ( NOT 24 In (VAR9)) 
            AND ( NOT 24 In (VAR10)) 
            AND ( NOT 24 In (VAR11)) 
            AND ( NOT 24 In (VAR12)) 
            AND ( NOT 25 In (VAR4)) 
            AND ( NOT 25 In (VAR5)) 
            AND ( NOT 25 In (VAR6)) 
            AND ( NOT 25 In (VAR7)) 
            AND ( NOT 25 In (VAR8)) 
            AND ( NOT 25 In (VAR9)) 
            AND ( NOT 25 In (VAR10)) 
            AND ( NOT 25 In (VAR11)) 
            AND ( NOT 25 In (VAR12))
        ) 
        AND 
        (
            ( 81 In (VAR4)) 
            OR ( 81 In (VAR5)) 
            OR ( 81 In (VAR6)) 
            OR ( 81 In (VAR7)) 
            OR ( 81 In (VAR8)) 
            OR ( 81 In (VAR9)) 
            OR ( 81 In (VAR10)) 
            OR ( 81 In (VAR11)) 
            OR ( 81 In (VAR12)) 
            OR ( 183 In (VAR4)) 
            OR ( 183 In (VAR5)) 
            OR ( 183 In (VAR6)) 
            OR ( 183 In (VAR7)) 
            OR ( 183 In (VAR8)) 
            OR ( 183 In (VAR9)) 
            OR ( 183 In (VAR10)) 
            OR ( 183 In (VAR11)) 
            OR ( 183 In (VAR12))
        )
    )
) 
AND NOT 
(
    ( 174 In (VAR13)) 
    OR ( 174 In (VAR14)) 
    OR ( 174 In (VAR15)) 
    OR ( 174 In (VAR16)) 
    OR ( 174 In (VAR17)) 
    OR ( 174 In (VAR18)) 
    OR ( 174 In (VAR19)) 
    OR ( 174 In (VAR20)) 
    OR ( 174 In (VAR21)) 
    OR ( 174 In (VAR22)) 
    OR ( 174 In (VAR23)) 
    OR ( 174 In (VAR24)) 
    OR ( 174 In (VAR25)) 
    OR ( 174 In (VAR26)) 
    OR ( 174 In (VAR27)) 
    OR ( 174 In (VAR28)) 
    OR ( 174 In (VAR29)) 
    OR ( 174 In (VAR30)) 
    OR ( 306 In (VAR13)) 
    OR ( 306 In (VAR14)) 
    OR ( 306 In (VAR15)) 
    OR ( 306 In (VAR16)) 
    OR ( 306 In (VAR17)) 
    OR ( 306 In (VAR18)) 
    OR ( 306 In (VAR19)) 
    OR ( 306 In (VAR20)) 
    OR ( 306 In (VAR21)) 
    OR ( 306 In (VAR22)) 
    OR ( 306 In (VAR23)) 
    OR ( 306 In (VAR24)) 
    OR ( 306 In (VAR25)) 
    OR ( 306 In (VAR26)) 
    OR ( 306 In (VAR27)) 
    OR ( 306 In (VAR28)) 
    OR ( 306 In (VAR29)) 
    OR ( 306 In (VAR30)) 
    OR ( 492 In (VAR13)) 
    OR ( 492 In (VAR14)) 
    OR ( 492 In (VAR15)) 
    OR ( 492 In (VAR16)) 
    OR ( 492 In (VAR17)) 
    OR ( 492 In (VAR18)) 
    OR ( 492 In (VAR19)) 
    OR ( 492 In (VAR20))
    OR ( 492 In (VAR21)) 
    OR ( 492 In (VAR22)) 
    OR ( 492 In (VAR23)) 
    OR ( 492 In (VAR24)) 
    OR ( 492 In (VAR25)) 
    OR ( 492 In (VAR26)) 
    OR ( 492 In (VAR27)) 
    OR ( 492 In (VAR28)) 
    OR ( 492 In (VAR29)) 
    OR ( 492 In (VAR30))
)";
        private const String SmallExpression = "(4 ^ 3.4 * 18 - VAR1) * (14 / 3) + VAR2";
        private const String SmallBranching = "If(If(23 > 15 AND 3*7 = 21 OR (25/5 > 10 AND 6+8 = 14), If(2.1=2.1,(4 ^ 3.4 * 18 - VAR1),If(2.1=2.1,0,1)), (14 / 3) + VAR2) <> 0 or true, If(2.1 <> 2.1 AND 3.1=3.1 OF 6.2=6.7, 2.1, 3.1), If(2.1=2.1 AND 3.2=3.2 OR 3.1<>3.1 OR 2.1<>2.3,3, 4))";

        [Test(Description = "Compile complicated expressions")]
        public void ProfileCompilationTime()
        {
            int expectedTime = 2000;
            int iterations = 10;

            var context = new ExpressionContext();
            context.Variables.ResolveVariableType += Variables_ResolveVariableType;
            context.Variables.ResolveVariableValue += Variables_ResolveVariableValue;
            Stopwatch sw;

            
            sw = new Stopwatch();
            sw.Start();

            for (int i = 0; i < iterations - 1; i++)
            {
                CreateDynamicExpression(BigExpression, context);
            }
            sw.Stop();
            this.PrintSpeedMessage("Compile Big", iterations, sw);
            Assert.Less(sw.ElapsedMilliseconds, expectedTime, "Test time above expected value");
            

            iterations = 100;
            expectedTime = 100;

            sw = new Stopwatch();
            sw.Start();

            for (int i = 0; i < iterations - 1; i++)
            {
                IDynamicExpression e = this.CreateDynamicExpression(SmallExpression, context);

            }
            sw.Stop();
            this.PrintSpeedMessage("Compile Small", iterations, sw);
            Assert.Less(sw.ElapsedMilliseconds, expectedTime, "Test time above expected value");

            iterations = 100;
            expectedTime = 100;

            sw = new Stopwatch();
            sw.Start();

            for (int i = 0; i < iterations - 1; i++)
            {
                IDynamicExpression e = this.CreateDynamicExpression(SmallExpression, context);

            }
            sw.Stop();
            this.PrintSpeedMessage("Compile Small Branching", iterations, sw);
            Assert.Less(sw.ElapsedMilliseconds, expectedTime, "Test time above expected value");
        }


        private static void Variables_ResolveVariableType(object sender, ResolveVariableTypeEventArgs e)
        {
            if (e.VariableName.StartsWith("VARBOOL"))
            {
                e.VariableType = typeof(bool);
            }
            else
            {
                e.VariableType = typeof(int);
            }
        }

        private static void Variables_ResolveVariableValue(object sender, ResolveVariableValueEventArgs e)
        {
            if (e.VariableType == typeof(bool))
            {
                e.VariableValue = false;
            }
            else
            {
                e.VariableValue = 0;
            }
        }

        private void PrintSpeedMessage(string title, int iterations, Stopwatch sw)
        {
            this.WriteMessage("{0}: {1:n0} iterations in {2:n2}ms = {3:n2} iterations/sec", title, iterations, sw.ElapsedMilliseconds, iterations*1000 / (sw.ElapsedMilliseconds));
        }
    }
}
