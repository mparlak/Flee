using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Flee.CalcEngine.PublicTypes;
using Flee.PublicTypes;
using NUnit.Framework;

namespace Flee.Test.CalcEngineTests
{
    [TestFixture]
    public class LongScriptTests
    {
        private SimpleCalcEngine _myEngine;
        public LongScriptTests()
        {
            var engine = new SimpleCalcEngine();
            var context = new ExpressionContext();
            context.Imports.AddType(typeof(Math));
			//            context.Imports.AddType(typeof(Math), "math");

			//' add convert methods e.g. .ToInt64, .ToString, .ToDateTime...  https://msdn.microsoft.com/en-us/library/system.convert.aspx?f=255&MSPPError=-2147217396 
			context.Imports.AddType(typeof(Convert));

			engine.Context = context;
            _myEngine = engine;
        }

        [Test]
        public void LongScriptWithManyFunctions()
        {
			//var script = System.IO.File.ReadAllText(@"test\Flee.Test\TestScripts\LongScriptWithManyFunctions.js");
			var script = @"If((""LongTextToPushScriptLengthOver256CharactersJustToMakeSureItDoesntMatter"")=""C"", 
	(
	If((""D"") = ""E"",
			2,
			3
		)
	),
    (
	If((""D"") = ""E"",
			Ceiling((((((4000) / 46.228) + 8) * 46.228) * 3) + 5126) + 1471 / 304.8 + 20,
			Ceiling(((((((((4000) / 46.228) + 8) * 46.228) * 3) + 5126) + 1217) / 304.8) + 20)
		)
	)
)";
			
			var expr = _myEngine.Context.CompileDynamic(script);
			var result = expr.Evaluate();

			Assert.AreEqual(84.0d, result);
		}


		[Test]
		public void FailingLongScriptWithManyFunctions()
		{
			//var script = System.IO.File.ReadAllText(@"test\Flee.Test\TestScripts\FailingLongScriptWithManyFunctions.js");
			var script = @"
If(""A"" = ""A"",
			(
				If((""LongTextToPushScriptLengthOver256CharactersJustToMakeSureItDoesntMatter"") = ""C"",
					(
						If((""D"") = ""E"",
							2,
							3
						)
					),
					(
						If((""D"") = ""E"",
							Ceiling((((((4000) / 46.228) + 8) * 46.228) * 3) + 5126) + 1471 / 304.8 + 20,
							Ceiling(((((((((4000) / 46.228) + 8) * 46.228) * 3) + 5126) + 1217) / 304.8) + 20)
						)
					)
				)
	),0
  )
";
			var expr = _myEngine.Context.CompileDynamic(script);
			var result = expr.Evaluate();

			Assert.AreEqual(84.0d, result);
		}

		[Test]
		public void NestedConditionalsForLongBranches()
		{
			//var script = System.IO.File.ReadAllText(@"test\Flee.Test\TestScripts\NestedConditionals.js");
			var script = @"IF(2.1 <> 2.1, 
IF(2.1 > 2.1, 2.1, 
IF(2.1 > 2.1 AND 2.1 <= 2.1, 2.1, 
IF(2.1 > 2.1 AND 2.1 <= 2.1, 2.1, 2.1))), 
IF(2.1 > 2.1, 2.1, 
IF(2.1 > 2.1 AND 2.1 <= 2.1, 2.1, 
IF(2.1 > 2.1 AND 2.1 <= 2.1, 2.1, 2.1))))";
			var expr = _myEngine.Context.CompileDynamic(script);
			var result = expr.Evaluate();

			Assert.AreEqual(2.1d, Convert.ToDecimal(result));
		}

		[Test]
		public void ShortCircuitLongBranches()
		{
			//var script = System.IO.File.ReadAllText(@"test\Flee.Test\TestScripts\NestedConditionals.js");
			var script = @"IF(
1 = 2 AND (16 * 24 + 8 * -1 < 0 OR 1+1+1+1+1+1+1+1+1+1+1+1+2+3+4+5+6+7+8+9+1+2+3+4+5+6+7+8+9+1+2+3+4+5+6+7+8+9+1+2+3*3-900 < 0)
AND (5*6+13-6*9-3+1+2+3+4+5+6+7+8 = 5+6+7+8+9+1+2+3+4+5+6+1+2+3+4+9-48 OR 6+5+2+3+8+1*9-6*7 > 8+6*4*(15-6)*(5+1+1+1+1+1+1+1+2))
,
1.4d,2.6d
)";
			var expr = _myEngine.Context.CompileDynamic(script);
			var result = expr.Evaluate();

			Assert.AreEqual(2.6d, result);
		}



	}
}
