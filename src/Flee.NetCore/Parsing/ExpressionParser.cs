using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Flee.Parsing;
using Flee.Parsing.grammatica_1._5.alpha2.PerCederberg.Grammatica.Runtime;
using Flee.PublicTypes;

namespace Flee.Parsing
{
    /// <summary>
    /// A token stream parser.
    /// </summary>
    internal class ExpressionParser : RecursiveDescentParser
    {
        private enum SynteticPatterns
        {
            SUBPRODUCTION_1 = 3001,
            SUBPRODUCTION_2 = 3002,
            SUBPRODUCTION_3 = 3003,
            SUBPRODUCTION_4 = 3004,
            SUBPRODUCTION_5 = 3005,
            SUBPRODUCTION_6 = 3006,
            SUBPRODUCTION_7 = 3007,
            SUBPRODUCTION_8 = 3008,
            SUBPRODUCTION_9 = 3009,
            SUBPRODUCTION_10 = 3010,
            SUBPRODUCTION_11 = 3011,
            SUBPRODUCTION_12 = 3012,
            SUBPRODUCTION_13 = 3013,
            SUBPRODUCTION_14 = 3014,
            SUBPRODUCTION_15 = 3015,
            SUBPRODUCTION_16 = 3016
        }

        public ExpressionParser(TextReader input, Analyzer analyzer, ExpressionContext context) : base(new ExpressionTokenizer(input, context), analyzer)
        {
            CreatePatterns();
        }

        public ExpressionParser(TextReader input) : base(new ExpressionTokenizer(input))
        {
            CreatePatterns();
        }

        public ExpressionParser(TextReader input, Analyzer analyzer) : base(new ExpressionTokenizer(input), analyzer)
        {
            CreatePatterns();
        }

        private void CreatePatterns()
        {
            ProductionPattern pattern = default(ProductionPattern);
            ProductionPatternAlternative alt = default(ProductionPatternAlternative);

            pattern = new ProductionPattern(Convert.ToInt32(ExpressionConstants.EXPRESSION), "Expression");
            alt = new ProductionPatternAlternative();
            alt.AddProduction(Convert.ToInt32(ExpressionConstants.XOR_EXPRESSION), 1, 1);
            pattern.AddAlternative(alt);
            AddPattern(pattern);

            pattern = new ProductionPattern(Convert.ToInt32(ExpressionConstants.XOR_EXPRESSION), "XorExpression");
            alt = new ProductionPatternAlternative();
            alt.AddProduction(Convert.ToInt32(ExpressionConstants.OR_EXPRESSION), 1, 1);
            alt.AddProduction(Convert.ToInt32(SynteticPatterns.SUBPRODUCTION_1), 0, -1);
            pattern.AddAlternative(alt);
            AddPattern(pattern);

            pattern = new ProductionPattern(Convert.ToInt32(ExpressionConstants.OR_EXPRESSION), "OrExpression");
            alt = new ProductionPatternAlternative();
            alt.AddProduction(Convert.ToInt32(ExpressionConstants.AND_EXPRESSION), 1, 1);
            alt.AddProduction(Convert.ToInt32(SynteticPatterns.SUBPRODUCTION_2), 0, -1);
            pattern.AddAlternative(alt);
            AddPattern(pattern);

            pattern = new ProductionPattern(Convert.ToInt32(ExpressionConstants.AND_EXPRESSION), "AndExpression");
            alt = new ProductionPatternAlternative();
            alt.AddProduction(Convert.ToInt32(ExpressionConstants.NOT_EXPRESSION), 1, 1);
            alt.AddProduction(Convert.ToInt32(SynteticPatterns.SUBPRODUCTION_3), 0, -1);
            pattern.AddAlternative(alt);
            AddPattern(pattern);

            pattern = new ProductionPattern(Convert.ToInt32(ExpressionConstants.NOT_EXPRESSION), "NotExpression");
            alt = new ProductionPatternAlternative();
            alt.AddToken(Convert.ToInt32(ExpressionConstants.NOT), 0, 1);
            alt.AddProduction(Convert.ToInt32(ExpressionConstants.IN_EXPRESSION), 1, 1);
            pattern.AddAlternative(alt);
            AddPattern(pattern);

            pattern = new ProductionPattern(Convert.ToInt32(ExpressionConstants.IN_EXPRESSION), "InExpression");
            alt = new ProductionPatternAlternative();
            alt.AddProduction(Convert.ToInt32(ExpressionConstants.COMPARE_EXPRESSION), 1, 1);
            alt.AddProduction(Convert.ToInt32(SynteticPatterns.SUBPRODUCTION_4), 0, 1);
            pattern.AddAlternative(alt);
            AddPattern(pattern);

            pattern = new ProductionPattern(Convert.ToInt32(ExpressionConstants.IN_TARGET_EXPRESSION), "InTargetExpression");
            alt = new ProductionPatternAlternative();
            alt.AddProduction(Convert.ToInt32(ExpressionConstants.FIELD_PROPERTY_EXPRESSION), 1, 1);
            pattern.AddAlternative(alt);
            alt = new ProductionPatternAlternative();
            alt.AddProduction(Convert.ToInt32(ExpressionConstants.IN_LIST_TARGET_EXPRESSION), 1, 1);
            pattern.AddAlternative(alt);
            AddPattern(pattern);

            pattern = new ProductionPattern(Convert.ToInt32(ExpressionConstants.IN_LIST_TARGET_EXPRESSION), "InListTargetExpression");
            alt = new ProductionPatternAlternative();
            alt.AddToken(Convert.ToInt32(ExpressionConstants.LEFT_PAREN), 1, 1);
            alt.AddProduction(Convert.ToInt32(ExpressionConstants.ARGUMENT_LIST), 1, 1);
            alt.AddToken(Convert.ToInt32(ExpressionConstants.RIGHT_PAREN), 1, 1);
            pattern.AddAlternative(alt);
            AddPattern(pattern);

            pattern = new ProductionPattern(Convert.ToInt32(ExpressionConstants.COMPARE_EXPRESSION), "CompareExpression");
            alt = new ProductionPatternAlternative();
            alt.AddProduction(Convert.ToInt32(ExpressionConstants.SHIFT_EXPRESSION), 1, 1);
            alt.AddProduction(Convert.ToInt32(SynteticPatterns.SUBPRODUCTION_6), 0, -1);
            pattern.AddAlternative(alt);
            AddPattern(pattern);

            pattern = new ProductionPattern(Convert.ToInt32(ExpressionConstants.SHIFT_EXPRESSION), "ShiftExpression");
            alt = new ProductionPatternAlternative();
            alt.AddProduction(Convert.ToInt32(ExpressionConstants.ADDITIVE_EXPRESSION), 1, 1);
            alt.AddProduction(Convert.ToInt32(SynteticPatterns.SUBPRODUCTION_8), 0, -1);
            pattern.AddAlternative(alt);
            AddPattern(pattern);

            pattern = new ProductionPattern(Convert.ToInt32(ExpressionConstants.ADDITIVE_EXPRESSION), "AdditiveExpression");
            alt = new ProductionPatternAlternative();
            alt.AddProduction(Convert.ToInt32(ExpressionConstants.MULTIPLICATIVE_EXPRESSION), 1, 1);
            alt.AddProduction(Convert.ToInt32(SynteticPatterns.SUBPRODUCTION_10), 0, -1);
            pattern.AddAlternative(alt);
            AddPattern(pattern);

            pattern = new ProductionPattern(Convert.ToInt32(ExpressionConstants.MULTIPLICATIVE_EXPRESSION), "MultiplicativeExpression");
            alt = new ProductionPatternAlternative();
            alt.AddProduction(Convert.ToInt32(ExpressionConstants.POWER_EXPRESSION), 1, 1);
            alt.AddProduction(Convert.ToInt32(SynteticPatterns.SUBPRODUCTION_12), 0, -1);
            pattern.AddAlternative(alt);
            AddPattern(pattern);

            pattern = new ProductionPattern(Convert.ToInt32(ExpressionConstants.POWER_EXPRESSION), "PowerExpression");
            alt = new ProductionPatternAlternative();
            alt.AddProduction(Convert.ToInt32(ExpressionConstants.NEGATE_EXPRESSION), 1, 1);
            alt.AddProduction(Convert.ToInt32(SynteticPatterns.SUBPRODUCTION_13), 0, -1);
            pattern.AddAlternative(alt);
            AddPattern(pattern);

            pattern = new ProductionPattern(Convert.ToInt32(ExpressionConstants.NEGATE_EXPRESSION), "NegateExpression");
            alt = new ProductionPatternAlternative();
            alt.AddToken(Convert.ToInt32(ExpressionConstants.SUB), 0, 1);
            alt.AddProduction(Convert.ToInt32(ExpressionConstants.MEMBER_EXPRESSION), 1, 1);
            pattern.AddAlternative(alt);
            AddPattern(pattern);

            pattern = new ProductionPattern(Convert.ToInt32(ExpressionConstants.MEMBER_EXPRESSION), "MemberExpression");
            alt = new ProductionPatternAlternative();
            alt.AddProduction(Convert.ToInt32(ExpressionConstants.BASIC_EXPRESSION), 1, 1);
            alt.AddProduction(Convert.ToInt32(SynteticPatterns.SUBPRODUCTION_14), 0, -1);
            pattern.AddAlternative(alt);
            AddPattern(pattern);

            pattern = new ProductionPattern(Convert.ToInt32(ExpressionConstants.MEMBER_ACCESS_EXPRESSION), "MemberAccessExpression");
            alt = new ProductionPatternAlternative();
            alt.AddToken(Convert.ToInt32(ExpressionConstants.DOT), 1, 1);
            alt.AddProduction(Convert.ToInt32(ExpressionConstants.MEMBER_FUNCTION_EXPRESSION), 1, 1);
            pattern.AddAlternative(alt);
            AddPattern(pattern);

            pattern = new ProductionPattern(Convert.ToInt32(ExpressionConstants.BASIC_EXPRESSION), "BasicExpression");
            alt = new ProductionPatternAlternative();
            alt.AddProduction(Convert.ToInt32(ExpressionConstants.LITERAL_EXPRESSION), 1, 1);
            pattern.AddAlternative(alt);
            alt = new ProductionPatternAlternative();
            alt.AddProduction(Convert.ToInt32(ExpressionConstants.EXPRESSION_GROUP), 1, 1);
            pattern.AddAlternative(alt);
            alt = new ProductionPatternAlternative();
            alt.AddProduction(Convert.ToInt32(ExpressionConstants.MEMBER_FUNCTION_EXPRESSION), 1, 1);
            pattern.AddAlternative(alt);
            alt = new ProductionPatternAlternative();
            alt.AddProduction(Convert.ToInt32(ExpressionConstants.SPECIAL_FUNCTION_EXPRESSION), 1, 1);
            pattern.AddAlternative(alt);
            AddPattern(pattern);

            pattern = new ProductionPattern(Convert.ToInt32(ExpressionConstants.MEMBER_FUNCTION_EXPRESSION), "MemberFunctionExpression");
            alt = new ProductionPatternAlternative();
            alt.AddProduction(Convert.ToInt32(ExpressionConstants.FIELD_PROPERTY_EXPRESSION), 1, 1);
            pattern.AddAlternative(alt);
            alt = new ProductionPatternAlternative();
            alt.AddProduction(Convert.ToInt32(ExpressionConstants.FUNCTION_CALL_EXPRESSION), 1, 1);
            pattern.AddAlternative(alt);
            AddPattern(pattern);

            pattern = new ProductionPattern(Convert.ToInt32(ExpressionConstants.FIELD_PROPERTY_EXPRESSION), "FieldPropertyExpression");
            alt = new ProductionPatternAlternative();
            alt.AddToken(Convert.ToInt32(ExpressionConstants.IDENTIFIER), 1, 1);
            pattern.AddAlternative(alt);
            AddPattern(pattern);

            pattern = new ProductionPattern(Convert.ToInt32(ExpressionConstants.SPECIAL_FUNCTION_EXPRESSION), "SpecialFunctionExpression");
            alt = new ProductionPatternAlternative();
            alt.AddProduction(Convert.ToInt32(ExpressionConstants.IF_EXPRESSION), 1, 1);
            pattern.AddAlternative(alt);
            alt = new ProductionPatternAlternative();
            alt.AddProduction(Convert.ToInt32(ExpressionConstants.CAST_EXPRESSION), 1, 1);
            pattern.AddAlternative(alt);
            AddPattern(pattern);

            pattern = new ProductionPattern(Convert.ToInt32(ExpressionConstants.IF_EXPRESSION), "IfExpression");
            alt = new ProductionPatternAlternative();
            alt.AddToken(Convert.ToInt32(ExpressionConstants.IF), 1, 1);
            alt.AddToken(Convert.ToInt32(ExpressionConstants.LEFT_PAREN), 1, 1);
            alt.AddProduction(Convert.ToInt32(ExpressionConstants.EXPRESSION), 1, 1);
            alt.AddToken(Convert.ToInt32(ExpressionConstants.ARGUMENT_SEPARATOR), 1, 1);
            alt.AddProduction(Convert.ToInt32(ExpressionConstants.EXPRESSION), 1, 1);
            alt.AddToken(Convert.ToInt32(ExpressionConstants.ARGUMENT_SEPARATOR), 1, 1);
            alt.AddProduction(Convert.ToInt32(ExpressionConstants.EXPRESSION), 1, 1);
            alt.AddToken(Convert.ToInt32(ExpressionConstants.RIGHT_PAREN), 1, 1);
            pattern.AddAlternative(alt);
            AddPattern(pattern);

            pattern = new ProductionPattern(Convert.ToInt32(ExpressionConstants.CAST_EXPRESSION), "CastExpression");
            alt = new ProductionPatternAlternative();
            alt.AddToken(Convert.ToInt32(ExpressionConstants.CAST), 1, 1);
            alt.AddToken(Convert.ToInt32(ExpressionConstants.LEFT_PAREN), 1, 1);
            alt.AddProduction(Convert.ToInt32(ExpressionConstants.EXPRESSION), 1, 1);
            alt.AddToken(Convert.ToInt32(ExpressionConstants.ARGUMENT_SEPARATOR), 1, 1);
            alt.AddProduction(Convert.ToInt32(ExpressionConstants.CAST_TYPE_EXPRESSION), 1, 1);
            alt.AddToken(Convert.ToInt32(ExpressionConstants.RIGHT_PAREN), 1, 1);
            pattern.AddAlternative(alt);
            AddPattern(pattern);

            pattern = new ProductionPattern(Convert.ToInt32(ExpressionConstants.CAST_TYPE_EXPRESSION), "CastTypeExpression");
            alt = new ProductionPatternAlternative();
            alt.AddToken(Convert.ToInt32(ExpressionConstants.IDENTIFIER), 1, 1);
            alt.AddProduction(Convert.ToInt32(SynteticPatterns.SUBPRODUCTION_15), 0, -1);
            alt.AddToken(Convert.ToInt32(ExpressionConstants.ARRAY_BRACES), 0, 1);
            pattern.AddAlternative(alt);
            AddPattern(pattern);

            pattern = new ProductionPattern(Convert.ToInt32(ExpressionConstants.INDEX_EXPRESSION), "IndexExpression");
            alt = new ProductionPatternAlternative();
            alt.AddToken(Convert.ToInt32(ExpressionConstants.LEFT_BRACE), 1, 1);
            alt.AddProduction(Convert.ToInt32(ExpressionConstants.ARGUMENT_LIST), 1, 1);
            alt.AddToken(Convert.ToInt32(ExpressionConstants.RIGHT_BRACE), 1, 1);
            pattern.AddAlternative(alt);
            AddPattern(pattern);

            pattern = new ProductionPattern(Convert.ToInt32(ExpressionConstants.FUNCTION_CALL_EXPRESSION), "FunctionCallExpression");
            alt = new ProductionPatternAlternative();
            alt.AddToken(Convert.ToInt32(ExpressionConstants.IDENTIFIER), 1, 1);
            alt.AddToken(Convert.ToInt32(ExpressionConstants.LEFT_PAREN), 1, 1);
            alt.AddProduction(Convert.ToInt32(ExpressionConstants.ARGUMENT_LIST), 0, 1);
            alt.AddToken(Convert.ToInt32(ExpressionConstants.RIGHT_PAREN), 1, 1);
            pattern.AddAlternative(alt);
            AddPattern(pattern);

            pattern = new ProductionPattern(Convert.ToInt32(ExpressionConstants.ARGUMENT_LIST), "ArgumentList");
            alt = new ProductionPatternAlternative();
            alt.AddProduction(Convert.ToInt32(ExpressionConstants.EXPRESSION), 1, 1);
            alt.AddProduction(Convert.ToInt32(SynteticPatterns.SUBPRODUCTION_16), 0, -1);
            pattern.AddAlternative(alt);
            AddPattern(pattern);

            pattern = new ProductionPattern(Convert.ToInt32(ExpressionConstants.LITERAL_EXPRESSION), "LiteralExpression");
            alt = new ProductionPatternAlternative();
            alt.AddToken(Convert.ToInt32(ExpressionConstants.INTEGER), 1, 1);
            pattern.AddAlternative(alt);
            alt = new ProductionPatternAlternative();
            alt.AddToken(Convert.ToInt32(ExpressionConstants.REAL), 1, 1);
            pattern.AddAlternative(alt);
            alt = new ProductionPatternAlternative();
            alt.AddToken(Convert.ToInt32(ExpressionConstants.STRING_LITERAL), 1, 1);
            pattern.AddAlternative(alt);
            alt = new ProductionPatternAlternative();
            alt.AddProduction(Convert.ToInt32(ExpressionConstants.BOOLEAN_LITERAL_EXPRESSION), 1, 1);
            pattern.AddAlternative(alt);
            alt = new ProductionPatternAlternative();
            alt.AddToken(Convert.ToInt32(ExpressionConstants.HEX_LITERAL), 1, 1);
            pattern.AddAlternative(alt);
            alt = new ProductionPatternAlternative();
            alt.AddToken(Convert.ToInt32(ExpressionConstants.CHAR_LITERAL), 1, 1);
            pattern.AddAlternative(alt);
            alt = new ProductionPatternAlternative();
            alt.AddToken(Convert.ToInt32(ExpressionConstants.NULL_LITERAL), 1, 1);
            pattern.AddAlternative(alt);
            alt = new ProductionPatternAlternative();
            alt.AddToken(Convert.ToInt32(ExpressionConstants.DATETIME), 1, 1);
            pattern.AddAlternative(alt);
            alt = new ProductionPatternAlternative();
            alt.AddToken(Convert.ToInt32(ExpressionConstants.TIMESPAN), 1, 1);
            pattern.AddAlternative(alt);
            AddPattern(pattern);

            pattern = new ProductionPattern(Convert.ToInt32(ExpressionConstants.BOOLEAN_LITERAL_EXPRESSION), "BooleanLiteralExpression");
            alt = new ProductionPatternAlternative();
            alt.AddToken(Convert.ToInt32(ExpressionConstants.TRUE), 1, 1);
            pattern.AddAlternative(alt);
            alt = new ProductionPatternAlternative();
            alt.AddToken(Convert.ToInt32(ExpressionConstants.FALSE), 1, 1);
            pattern.AddAlternative(alt);
            AddPattern(pattern);

            pattern = new ProductionPattern(Convert.ToInt32(ExpressionConstants.EXPRESSION_GROUP), "ExpressionGroup");
            alt = new ProductionPatternAlternative();
            alt.AddToken(Convert.ToInt32(ExpressionConstants.LEFT_PAREN), 1, 1);
            alt.AddProduction(Convert.ToInt32(ExpressionConstants.EXPRESSION), 1, 1);
            alt.AddToken(Convert.ToInt32(ExpressionConstants.RIGHT_PAREN), 1, 1);
            pattern.AddAlternative(alt);
            AddPattern(pattern);

            pattern = new ProductionPattern(Convert.ToInt32(SynteticPatterns.SUBPRODUCTION_1), "Subproduction1");
            pattern.Synthetic = true;
            alt = new ProductionPatternAlternative();
            alt.AddToken(Convert.ToInt32(ExpressionConstants.XOR), 1, 1);
            alt.AddProduction(Convert.ToInt32(ExpressionConstants.OR_EXPRESSION), 1, 1);
            pattern.AddAlternative(alt);
            AddPattern(pattern);

            pattern = new ProductionPattern(Convert.ToInt32(SynteticPatterns.SUBPRODUCTION_2), "Subproduction2");
            pattern.Synthetic = true;
            alt = new ProductionPatternAlternative();
            alt.AddToken(Convert.ToInt32(ExpressionConstants.OR), 1, 1);
            alt.AddProduction(Convert.ToInt32(ExpressionConstants.AND_EXPRESSION), 1, 1);
            pattern.AddAlternative(alt);
            AddPattern(pattern);

            pattern = new ProductionPattern(Convert.ToInt32(SynteticPatterns.SUBPRODUCTION_3), "Subproduction3");
            pattern.Synthetic = true;
            alt = new ProductionPatternAlternative();
            alt.AddToken(Convert.ToInt32(ExpressionConstants.AND), 1, 1);
            alt.AddProduction(Convert.ToInt32(ExpressionConstants.NOT_EXPRESSION), 1, 1);
            pattern.AddAlternative(alt);
            AddPattern(pattern);

            pattern = new ProductionPattern(Convert.ToInt32(SynteticPatterns.SUBPRODUCTION_4), "Subproduction4");
            pattern.Synthetic = true;
            alt = new ProductionPatternAlternative();
            alt.AddToken(Convert.ToInt32(ExpressionConstants.IN), 1, 1);
            alt.AddProduction(Convert.ToInt32(ExpressionConstants.IN_TARGET_EXPRESSION), 1, 1);
            pattern.AddAlternative(alt);
            AddPattern(pattern);

            pattern = new ProductionPattern(Convert.ToInt32(SynteticPatterns.SUBPRODUCTION_5), "Subproduction5");
            pattern.Synthetic = true;
            alt = new ProductionPatternAlternative();
            alt.AddToken(Convert.ToInt32(ExpressionConstants.EQ), 1, 1);
            pattern.AddAlternative(alt);
            alt = new ProductionPatternAlternative();
            alt.AddToken(Convert.ToInt32(ExpressionConstants.GT), 1, 1);
            pattern.AddAlternative(alt);
            alt = new ProductionPatternAlternative();
            alt.AddToken(Convert.ToInt32(ExpressionConstants.LT), 1, 1);
            pattern.AddAlternative(alt);
            alt = new ProductionPatternAlternative();
            alt.AddToken(Convert.ToInt32(ExpressionConstants.GTE), 1, 1);
            pattern.AddAlternative(alt);
            alt = new ProductionPatternAlternative();
            alt.AddToken(Convert.ToInt32(ExpressionConstants.LTE), 1, 1);
            pattern.AddAlternative(alt);
            alt = new ProductionPatternAlternative();
            alt.AddToken(Convert.ToInt32(ExpressionConstants.NE), 1, 1);
            pattern.AddAlternative(alt);
            AddPattern(pattern);

            pattern = new ProductionPattern(Convert.ToInt32(SynteticPatterns.SUBPRODUCTION_6), "Subproduction6");
            pattern.Synthetic = true;
            alt = new ProductionPatternAlternative();
            alt.AddProduction(Convert.ToInt32(SynteticPatterns.SUBPRODUCTION_5), 1, 1);
            alt.AddProduction(Convert.ToInt32(ExpressionConstants.SHIFT_EXPRESSION), 1, 1);
            pattern.AddAlternative(alt);
            AddPattern(pattern);

            pattern = new ProductionPattern(Convert.ToInt32(SynteticPatterns.SUBPRODUCTION_7), "Subproduction7");
            pattern.Synthetic = true;
            alt = new ProductionPatternAlternative();
            alt.AddToken(Convert.ToInt32(ExpressionConstants.LEFT_SHIFT), 1, 1);
            pattern.AddAlternative(alt);
            alt = new ProductionPatternAlternative();
            alt.AddToken(Convert.ToInt32(ExpressionConstants.RIGHT_SHIFT), 1, 1);
            pattern.AddAlternative(alt);
            AddPattern(pattern);

            pattern = new ProductionPattern(Convert.ToInt32(SynteticPatterns.SUBPRODUCTION_8), "Subproduction8");
            pattern.Synthetic = true;
            alt = new ProductionPatternAlternative();
            alt.AddProduction(Convert.ToInt32(SynteticPatterns.SUBPRODUCTION_7), 1, 1);
            alt.AddProduction(Convert.ToInt32(ExpressionConstants.ADDITIVE_EXPRESSION), 1, 1);
            pattern.AddAlternative(alt);
            AddPattern(pattern);

            pattern = new ProductionPattern(Convert.ToInt32(SynteticPatterns.SUBPRODUCTION_9), "Subproduction9");
            pattern.Synthetic = true;
            alt = new ProductionPatternAlternative();
            alt.AddToken(Convert.ToInt32(ExpressionConstants.ADD), 1, 1);
            pattern.AddAlternative(alt);
            alt = new ProductionPatternAlternative();
            alt.AddToken(Convert.ToInt32(ExpressionConstants.SUB), 1, 1);
            pattern.AddAlternative(alt);
            AddPattern(pattern);

            pattern = new ProductionPattern(Convert.ToInt32(SynteticPatterns.SUBPRODUCTION_10), "Subproduction10");
            pattern.Synthetic = true;
            alt = new ProductionPatternAlternative();
            alt.AddProduction(Convert.ToInt32(SynteticPatterns.SUBPRODUCTION_9), 1, 1);
            alt.AddProduction(Convert.ToInt32(ExpressionConstants.MULTIPLICATIVE_EXPRESSION), 1, 1);
            pattern.AddAlternative(alt);
            AddPattern(pattern);

            pattern = new ProductionPattern(Convert.ToInt32(SynteticPatterns.SUBPRODUCTION_11), "Subproduction11");
            pattern.Synthetic = true;
            alt = new ProductionPatternAlternative();
            alt.AddToken(Convert.ToInt32(ExpressionConstants.MUL), 1, 1);
            pattern.AddAlternative(alt);
            alt = new ProductionPatternAlternative();
            alt.AddToken(Convert.ToInt32(ExpressionConstants.DIV), 1, 1);
            pattern.AddAlternative(alt);
            alt = new ProductionPatternAlternative();
            alt.AddToken(Convert.ToInt32(ExpressionConstants.MOD), 1, 1);
            pattern.AddAlternative(alt);
            AddPattern(pattern);

            pattern = new ProductionPattern(Convert.ToInt32(SynteticPatterns.SUBPRODUCTION_12), "Subproduction12");
            pattern.Synthetic = true;
            alt = new ProductionPatternAlternative();
            alt.AddProduction(Convert.ToInt32(SynteticPatterns.SUBPRODUCTION_11), 1, 1);
            alt.AddProduction(Convert.ToInt32(ExpressionConstants.POWER_EXPRESSION), 1, 1);
            pattern.AddAlternative(alt);
            AddPattern(pattern);

            pattern = new ProductionPattern(Convert.ToInt32(SynteticPatterns.SUBPRODUCTION_13), "Subproduction13");
            pattern.Synthetic = true;
            alt = new ProductionPatternAlternative();
            alt.AddToken(Convert.ToInt32(ExpressionConstants.POWER), 1, 1);
            alt.AddProduction(Convert.ToInt32(ExpressionConstants.NEGATE_EXPRESSION), 1, 1);
            pattern.AddAlternative(alt);
            AddPattern(pattern);

            pattern = new ProductionPattern(Convert.ToInt32(SynteticPatterns.SUBPRODUCTION_14), "Subproduction14");
            pattern.Synthetic = true;
            alt = new ProductionPatternAlternative();
            alt.AddProduction(Convert.ToInt32(ExpressionConstants.MEMBER_ACCESS_EXPRESSION), 1, 1);
            pattern.AddAlternative(alt);
            alt = new ProductionPatternAlternative();
            alt.AddProduction(Convert.ToInt32(ExpressionConstants.INDEX_EXPRESSION), 1, 1);
            pattern.AddAlternative(alt);
            AddPattern(pattern);

            pattern = new ProductionPattern(Convert.ToInt32(SynteticPatterns.SUBPRODUCTION_15), "Subproduction15");
            pattern.Synthetic = true;
            alt = new ProductionPatternAlternative();
            alt.AddToken(Convert.ToInt32(ExpressionConstants.DOT), 1, 1);
            alt.AddToken(Convert.ToInt32(ExpressionConstants.IDENTIFIER), 1, 1);
            pattern.AddAlternative(alt);
            AddPattern(pattern);

            pattern = new ProductionPattern(Convert.ToInt32(SynteticPatterns.SUBPRODUCTION_16), "Subproduction16");
            pattern.Synthetic = true;
            alt = new ProductionPatternAlternative();
            alt.AddToken(Convert.ToInt32(ExpressionConstants.ARGUMENT_SEPARATOR), 1, 1);
            alt.AddProduction(Convert.ToInt32(ExpressionConstants.EXPRESSION), 1, 1);
            pattern.AddAlternative(alt);
            AddPattern(pattern);
        }
    }
}
