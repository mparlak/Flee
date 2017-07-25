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
    /// A character stream tokenizer.
    /// </summary>
    internal class ExpressionTokenizer : Tokenizer
    {
        private readonly ExpressionContext _myContext;
        
        public ExpressionTokenizer(TextReader input, ExpressionContext context) : base(input, true)
        {
            _myContext = context;
            CreatePatterns();
        }

        public ExpressionTokenizer(TextReader input) : base(input, true)
        {
            CreatePatterns();
        }

        private void CreatePatterns()
        {
            TokenPattern pattern = default(TokenPattern);
            CustomTokenPattern customPattern = default(CustomTokenPattern);

            pattern = new TokenPattern(Convert.ToInt32(ExpressionConstants.ADD), "ADD", TokenPattern.PatternType.STRING, "+");
            AddPattern(pattern);

            pattern = new TokenPattern(Convert.ToInt32(ExpressionConstants.SUB), "SUB", TokenPattern.PatternType.STRING, "-");
            AddPattern(pattern);

            pattern = new TokenPattern(Convert.ToInt32(ExpressionConstants.MUL), "MUL", TokenPattern.PatternType.STRING, "*");
            AddPattern(pattern);

            pattern = new TokenPattern(Convert.ToInt32(ExpressionConstants.DIV), "DIV", TokenPattern.PatternType.STRING, "/");
            AddPattern(pattern);

            pattern = new TokenPattern(Convert.ToInt32(ExpressionConstants.POWER), "POWER", TokenPattern.PatternType.STRING, "^");
            AddPattern(pattern);

            pattern = new TokenPattern(Convert.ToInt32(ExpressionConstants.MOD), "MOD", TokenPattern.PatternType.STRING, "%");
            AddPattern(pattern);

            pattern = new TokenPattern(Convert.ToInt32(ExpressionConstants.LEFT_PAREN), "LEFT_PAREN", TokenPattern.PatternType.STRING, "(");
            AddPattern(pattern);

            pattern = new TokenPattern(Convert.ToInt32(ExpressionConstants.RIGHT_PAREN), "RIGHT_PAREN", TokenPattern.PatternType.STRING, ")");
            AddPattern(pattern);

            pattern = new TokenPattern(Convert.ToInt32(ExpressionConstants.LEFT_BRACE), "LEFT_BRACE", TokenPattern.PatternType.STRING, "[");
            AddPattern(pattern);

            pattern = new TokenPattern(Convert.ToInt32(ExpressionConstants.RIGHT_BRACE), "RIGHT_BRACE", TokenPattern.PatternType.STRING, "]");
            AddPattern(pattern);

            pattern = new TokenPattern(Convert.ToInt32(ExpressionConstants.EQ), "EQ", TokenPattern.PatternType.STRING, "=");
            AddPattern(pattern);

            pattern = new TokenPattern(Convert.ToInt32(ExpressionConstants.LT), "LT", TokenPattern.PatternType.STRING, "<");
            AddPattern(pattern);

            pattern = new TokenPattern(Convert.ToInt32(ExpressionConstants.GT), "GT", TokenPattern.PatternType.STRING, ">");
            AddPattern(pattern);

            pattern = new TokenPattern(Convert.ToInt32(ExpressionConstants.LTE), "LTE", TokenPattern.PatternType.STRING, "<=");
            AddPattern(pattern);

            pattern = new TokenPattern(Convert.ToInt32(ExpressionConstants.GTE), "GTE", TokenPattern.PatternType.STRING, ">=");
            AddPattern(pattern);

            pattern = new TokenPattern(Convert.ToInt32(ExpressionConstants.NE), "NE", TokenPattern.PatternType.STRING, "<>");
            AddPattern(pattern);

            pattern = new TokenPattern(Convert.ToInt32(ExpressionConstants.AND), "AND", TokenPattern.PatternType.STRING, "AND");
            AddPattern(pattern);

            pattern = new TokenPattern(Convert.ToInt32(ExpressionConstants.OR), "OR", TokenPattern.PatternType.STRING, "OR");
            AddPattern(pattern);

            pattern = new TokenPattern(Convert.ToInt32(ExpressionConstants.XOR), "XOR", TokenPattern.PatternType.STRING, "XOR");
            AddPattern(pattern);

            pattern = new TokenPattern(Convert.ToInt32(ExpressionConstants.NOT), "NOT", TokenPattern.PatternType.STRING, "NOT");
            AddPattern(pattern);

            pattern = new TokenPattern(Convert.ToInt32(ExpressionConstants.IN), "IN", TokenPattern.PatternType.STRING, "in");
            AddPattern(pattern);

            pattern = new TokenPattern(Convert.ToInt32(ExpressionConstants.DOT), "DOT", TokenPattern.PatternType.STRING, ".");
            AddPattern(pattern);

            customPattern = new ArgumentSeparatorPattern(Convert.ToInt32(ExpressionConstants.ARGUMENT_SEPARATOR), "ARGUMENT_SEPARATOR", TokenPattern.PatternType.STRING, ",");
            customPattern.Initialize(Convert.ToInt32(ExpressionConstants.ARGUMENT_SEPARATOR), "ARGUMENT_SEPARATOR", TokenPattern.PatternType.STRING, ",", _myContext);
            AddPattern(customPattern);

            pattern = new TokenPattern(Convert.ToInt32(ExpressionConstants.ARRAY_BRACES), "ARRAY_BRACES", TokenPattern.PatternType.STRING, "[]");
            AddPattern(pattern);

            pattern = new TokenPattern(Convert.ToInt32(ExpressionConstants.LEFT_SHIFT), "LEFT_SHIFT", TokenPattern.PatternType.STRING, "<<");
            AddPattern(pattern);

            pattern = new TokenPattern(Convert.ToInt32(ExpressionConstants.RIGHT_SHIFT), "RIGHT_SHIFT", TokenPattern.PatternType.STRING, ">>");
            AddPattern(pattern);

            pattern = new TokenPattern(Convert.ToInt32(ExpressionConstants.WHITESPACE), "WHITESPACE", TokenPattern.PatternType.REGEXP, "\\s+");
            pattern.Ignore = true;
            AddPattern(pattern);

            pattern = new TokenPattern(Convert.ToInt32(ExpressionConstants.INTEGER), "INTEGER", TokenPattern.PatternType.REGEXP, "\\d+(u|l|ul|lu|f|m)?");
            AddPattern(pattern);

            customPattern = new RealPattern(Convert.ToInt32(ExpressionConstants.REAL), "REAL", TokenPattern.PatternType.REGEXP, "\\d{0}\\{1}\\d+([e][+-]\\d{{1,3}})?(d|f|m)?");
            customPattern.Initialize(Convert.ToInt32(ExpressionConstants.REAL), "REAL", TokenPattern.PatternType.REGEXP, "\\d{0}\\{1}\\d+([e][+-]\\d{{1,3}})?(d|f|m)?", _myContext);
            AddPattern(customPattern);

            pattern = new TokenPattern(Convert.ToInt32(ExpressionConstants.STRING_LITERAL), "STRING_LITERAL", TokenPattern.PatternType.REGEXP, "\"([^\"\\r\\n\\\\]|\\\\u[0-9a-f]{4}|\\\\[\\\\\"'trn])*\"");
            AddPattern(pattern);

            pattern = new TokenPattern(Convert.ToInt32(ExpressionConstants.CHAR_LITERAL), "CHAR_LITERAL", TokenPattern.PatternType.REGEXP, "'([^'\\r\\n\\\\]|\\\\u[0-9a-f]{4}|\\\\[\\\\\"'trn])'");
            AddPattern(pattern);

            pattern = new TokenPattern(Convert.ToInt32(ExpressionConstants.TRUE), "TRUE", TokenPattern.PatternType.STRING, "True");
            AddPattern(pattern);

            pattern = new TokenPattern(Convert.ToInt32(ExpressionConstants.FALSE), "FALSE", TokenPattern.PatternType.STRING, "False");
            AddPattern(pattern);

            pattern = new TokenPattern(Convert.ToInt32(ExpressionConstants.IDENTIFIER), "IDENTIFIER", TokenPattern.PatternType.REGEXP, "[a-z_]\\w*");
            AddPattern(pattern);

            pattern = new TokenPattern(Convert.ToInt32(ExpressionConstants.HEX_LITERAL), "HEX_LITERAL", TokenPattern.PatternType.REGEXP, "0x[0-9a-f]+(u|l|ul|lu)?");
            AddPattern(pattern);

            pattern = new TokenPattern(Convert.ToInt32(ExpressionConstants.NULL_LITERAL), "NULL_LITERAL", TokenPattern.PatternType.STRING, "null");
            AddPattern(pattern);

            pattern = new TokenPattern(Convert.ToInt32(ExpressionConstants.TIMESPAN), "TIMESPAN", TokenPattern.PatternType.REGEXP, "##(\\d+\\.)?\\d{2}:\\d{2}(:\\d{2}(\\.\\d{1,7})?)?#");
            AddPattern(pattern);

            pattern = new TokenPattern(Convert.ToInt32(ExpressionConstants.DATETIME), "DATETIME", TokenPattern.PatternType.REGEXP, "#[^#]+#");
            AddPattern(pattern);

            pattern = new TokenPattern(Convert.ToInt32(ExpressionConstants.IF), "IF", TokenPattern.PatternType.STRING, "if");
            AddPattern(pattern);

            pattern = new TokenPattern(Convert.ToInt32(ExpressionConstants.CAST), "CAST", TokenPattern.PatternType.STRING, "cast");
            AddPattern(pattern);
        }
    }
}
