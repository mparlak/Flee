using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Flee.PublicTypes;

namespace Flee.Test.ExpressionTests
{
    public class Core
    {
        protected IDynamicExpression CreateDynamicExpression(string expression, ExpressionContext context)
        {
            return context.CompileDynamic(expression);
        }

        protected void WriteMessage(string msg, params object[] args)
        {
            msg = String.Format(msg, args);
            Console.WriteLine(msg);
        }
    }
}
