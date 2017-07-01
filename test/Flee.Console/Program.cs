using Flee.CalcEngine.PublicTypes;
using Flee.PublicTypes;
using System;

namespace Flee.Console
{
    class Program
    {
        static void Main(string[] args)
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
            System.Console.WriteLine("Result : " + (result == ((100 * 2) + 1) * 2 ? "Success" : "Fail"));

            variables.Remove("x");
            variables.Add("x", 345);
            ce.Recalculate("a");
            result = ce.GetResult<int>("c");
            System.Console.WriteLine("Result : " + (result == ((345 * 2) + 1) * 2 ? "Success" : "Fail"));

            System.Console.ReadKey();
        }
    }
}
