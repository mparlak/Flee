using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using Flee.Parsing.grammatica_1._5.alpha2.PerCederberg.Grammatica.Runtime;

namespace Flee.Parsing
{
    /// <summary>
    /// A class providing callback methods for the parser.
    /// </summary>
    internal abstract class ExpressionAnalyzer : Analyzer
    {
        /// <summary>
        /// Called when entering a parse tree node.
        /// </summary>
        /// <param name="node"></param>
        public override void Enter(Node node)
        {
            switch (node.Id)
            {
                case (int)ExpressionConstants.ADD:
                    EnterAdd((Token)node);

                    break;
                case (int)ExpressionConstants.SUB:
                    EnterSub((Token)node);

                    break;
                case (int)ExpressionConstants.MUL:
                    EnterMul((Token)node);

                    break;
                case (int)ExpressionConstants.DIV:
                    EnterDiv((Token)node);

                    break;
                case (int)ExpressionConstants.POWER:
                    EnterPower((Token)node);

                    break;
                case (int)ExpressionConstants.MOD:
                    EnterMod((Token)node);

                    break;
                case (int)ExpressionConstants.LEFT_PAREN:
                    EnterLeftParen((Token)node);

                    break;
                case (int)ExpressionConstants.RIGHT_PAREN:
                    EnterRightParen((Token)node);

                    break;
                case (int)ExpressionConstants.LEFT_BRACE:
                    EnterLeftBrace((Token)node);

                    break;
                case (int)ExpressionConstants.RIGHT_BRACE:
                    EnterRightBrace((Token)node);

                    break;
                case (int)ExpressionConstants.EQ:
                    EnterEq((Token)node);

                    break;
                case (int)ExpressionConstants.LT:
                    EnterLt((Token)node);

                    break;
                case (int)ExpressionConstants.GT:
                    EnterGt((Token)node);

                    break;
                case (int)ExpressionConstants.LTE:
                    EnterLte((Token)node);

                    break;
                case (int)ExpressionConstants.GTE:
                    EnterGte((Token)node);

                    break;
                case (int)ExpressionConstants.NE:
                    EnterNe((Token)node);

                    break;
                case (int)ExpressionConstants.AND:
                    EnterAnd((Token)node);

                    break;
                case (int)ExpressionConstants.OR:
                    EnterOr((Token)node);

                    break;
                case (int)ExpressionConstants.XOR:
                    EnterXor((Token)node);

                    break;
                case (int)ExpressionConstants.NOT:
                    EnterNot((Token)node);

                    break;
                case (int)ExpressionConstants.IN:
                    EnterIn((Token)node);

                    break;
                case (int)ExpressionConstants.DOT:
                    EnterDot((Token)node);

                    break;
                case (int)ExpressionConstants.ARGUMENT_SEPARATOR:
                    EnterArgumentSeparator((Token)node);

                    break;
                case (int)ExpressionConstants.ARRAY_BRACES:
                    EnterArrayBraces((Token)node);

                    break;
                case (int)ExpressionConstants.LEFT_SHIFT:
                    EnterLeftShift((Token)node);

                    break;
                case (int)ExpressionConstants.RIGHT_SHIFT:
                    EnterRightShift((Token)node);

                    break;
                case (int)ExpressionConstants.INTEGER:
                    EnterInteger((Token)node);

                    break;
                case (int)ExpressionConstants.REAL:
                    EnterReal((Token)node);

                    break;
                case (int)ExpressionConstants.STRING_LITERAL:
                    EnterStringLiteral((Token)node);

                    break;
                case (int)ExpressionConstants.CHAR_LITERAL:
                    EnterCharLiteral((Token)node);

                    break;
                case (int)ExpressionConstants.TRUE:
                    EnterTrue((Token)node);

                    break;
                case (int)ExpressionConstants.FALSE:
                    EnterFalse((Token)node);

                    break;
                case (int)ExpressionConstants.IDENTIFIER:
                    EnterIdentifier((Token)node);

                    break;
                case (int)ExpressionConstants.HEX_LITERAL:
                    EnterHexLiteral((Token)node);

                    break;
                case (int)ExpressionConstants.NULL_LITERAL:
                    EnterNullLiteral((Token)node);

                    break;
                case (int)ExpressionConstants.TIMESPAN:
                    EnterTimespan((Token)node);

                    break;
                case (int)ExpressionConstants.DATETIME:
                    EnterDatetime((Token)node);

                    break;
                case (int)ExpressionConstants.IF:
                    EnterIf((Token)node);

                    break;
                case (int)ExpressionConstants.CAST:
                    EnterCast((Token)node);

                    break;
                case (int)ExpressionConstants.EXPRESSION:
                    EnterExpression((Production)node);

                    break;
                case (int)ExpressionConstants.XOR_EXPRESSION:
                    EnterXorExpression((Production)node);

                    break;
                case (int)ExpressionConstants.OR_EXPRESSION:
                    EnterOrExpression((Production)node);

                    break;
                case (int)ExpressionConstants.AND_EXPRESSION:
                    EnterAndExpression((Production)node);

                    break;
                case (int)ExpressionConstants.NOT_EXPRESSION:
                    EnterNotExpression((Production)node);

                    break;
                case (int)ExpressionConstants.IN_EXPRESSION:
                    EnterInExpression((Production)node);

                    break;
                case (int)ExpressionConstants.IN_TARGET_EXPRESSION:
                    EnterInTargetExpression((Production)node);

                    break;
                case (int)ExpressionConstants.IN_LIST_TARGET_EXPRESSION:
                    EnterInListTargetExpression((Production)node);

                    break;
                case (int)ExpressionConstants.COMPARE_EXPRESSION:
                    EnterCompareExpression((Production)node);

                    break;
                case (int)ExpressionConstants.SHIFT_EXPRESSION:
                    EnterShiftExpression((Production)node);

                    break;
                case (int)ExpressionConstants.ADDITIVE_EXPRESSION:
                    EnterAdditiveExpression((Production)node);

                    break;
                case (int)ExpressionConstants.MULTIPLICATIVE_EXPRESSION:
                    EnterMultiplicativeExpression((Production)node);

                    break;
                case (int)ExpressionConstants.POWER_EXPRESSION:
                    EnterPowerExpression((Production)node);

                    break;
                case (int)ExpressionConstants.NEGATE_EXPRESSION:
                    EnterNegateExpression((Production)node);

                    break;
                case (int)ExpressionConstants.MEMBER_EXPRESSION:
                    EnterMemberExpression((Production)node);

                    break;
                case (int)ExpressionConstants.MEMBER_ACCESS_EXPRESSION:
                    EnterMemberAccessExpression((Production)node);

                    break;
                case (int)ExpressionConstants.BASIC_EXPRESSION:
                    EnterBasicExpression((Production)node);

                    break;
                case (int)ExpressionConstants.MEMBER_FUNCTION_EXPRESSION:
                    EnterMemberFunctionExpression((Production)node);

                    break;
                case (int)ExpressionConstants.FIELD_PROPERTY_EXPRESSION:
                    EnterFieldPropertyExpression((Production)node);

                    break;
                case (int)ExpressionConstants.SPECIAL_FUNCTION_EXPRESSION:
                    EnterSpecialFunctionExpression((Production)node);

                    break;
                case (int)ExpressionConstants.IF_EXPRESSION:
                    EnterIfExpression((Production)node);

                    break;
                case (int)ExpressionConstants.CAST_EXPRESSION:
                    EnterCastExpression((Production)node);

                    break;
                case (int)ExpressionConstants.CAST_TYPE_EXPRESSION:
                    EnterCastTypeExpression((Production)node);

                    break;
                case (int)ExpressionConstants.INDEX_EXPRESSION:
                    EnterIndexExpression((Production)node);

                    break;
                case (int)ExpressionConstants.FUNCTION_CALL_EXPRESSION:
                    EnterFunctionCallExpression((Production)node);

                    break;
                case (int)ExpressionConstants.ARGUMENT_LIST:
                    EnterArgumentList((Production)node);

                    break;
                case (int)ExpressionConstants.LITERAL_EXPRESSION:
                    EnterLiteralExpression((Production)node);

                    break;
                case (int)ExpressionConstants.BOOLEAN_LITERAL_EXPRESSION:
                    EnterBooleanLiteralExpression((Production)node);

                    break;
                case (int)ExpressionConstants.EXPRESSION_GROUP:
                    EnterExpressionGroup((Production)node);

                    break;
            }
        }

        /// <summary>
        /// Called when exiting a parse tree node.the node being exited the node to add to the parse tree, or null if no parse tree should be created<
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        public override Node Exit(Node node)
        {
            switch (node.Id)
            {
                case (int)ExpressionConstants.ADD:

                    return ExitAdd((Token)node);
                case (int)ExpressionConstants.SUB:

                    return ExitSub((Token)node);
                case (int)ExpressionConstants.MUL:

                    return ExitMul((Token)node);
                case (int)ExpressionConstants.DIV:

                    return ExitDiv((Token)node);
                case (int)ExpressionConstants.POWER:

                    return ExitPower((Token)node);
                case (int)ExpressionConstants.MOD:

                    return ExitMod((Token)node);
                case (int)ExpressionConstants.LEFT_PAREN:

                    return ExitLeftParen((Token)node);
                case (int)ExpressionConstants.RIGHT_PAREN:

                    return ExitRightParen((Token)node);
                case (int)ExpressionConstants.LEFT_BRACE:

                    return ExitLeftBrace((Token)node);
                case (int)ExpressionConstants.RIGHT_BRACE:

                    return ExitRightBrace((Token)node);
                case (int)ExpressionConstants.EQ:

                    return ExitEq((Token)node);
                case (int)ExpressionConstants.LT:

                    return ExitLt((Token)node);
                case (int)ExpressionConstants.GT:

                    return ExitGt((Token)node);
                case (int)ExpressionConstants.LTE:

                    return ExitLte((Token)node);
                case (int)ExpressionConstants.GTE:

                    return ExitGte((Token)node);
                case (int)ExpressionConstants.NE:

                    return ExitNe((Token)node);
                case (int)ExpressionConstants.AND:

                    return ExitAnd((Token)node);
                case (int)ExpressionConstants.OR:

                    return ExitOr((Token)node);
                case (int)ExpressionConstants.XOR:

                    return ExitXor((Token)node);
                case (int)ExpressionConstants.NOT:

                    return ExitNot((Token)node);
                case (int)ExpressionConstants.IN:

                    return ExitIn((Token)node);
                case (int)ExpressionConstants.DOT:

                    return ExitDot((Token)node);
                case (int)ExpressionConstants.ARGUMENT_SEPARATOR:

                    return ExitArgumentSeparator((Token)node);
                case (int)ExpressionConstants.ARRAY_BRACES:

                    return ExitArrayBraces((Token)node);
                case (int)ExpressionConstants.LEFT_SHIFT:

                    return ExitLeftShift((Token)node);
                case (int)ExpressionConstants.RIGHT_SHIFT:

                    return ExitRightShift((Token)node);
                case (int)ExpressionConstants.INTEGER:

                    return ExitInteger((Token)node);
                case (int)ExpressionConstants.REAL:

                    return ExitReal((Token)node);
                case (int)ExpressionConstants.STRING_LITERAL:

                    return ExitStringLiteral((Token)node);
                case (int)ExpressionConstants.CHAR_LITERAL:

                    return ExitCharLiteral((Token)node);
                case (int)ExpressionConstants.TRUE:

                    return ExitTrue((Token)node);
                case (int)ExpressionConstants.FALSE:

                    return ExitFalse((Token)node);
                case (int)ExpressionConstants.IDENTIFIER:

                    return ExitIdentifier((Token)node);
                case (int)ExpressionConstants.HEX_LITERAL:

                    return ExitHexliteral((Token)node);
                case (int)ExpressionConstants.NULL_LITERAL:

                    return ExitNullLiteral((Token)node);
                case (int)ExpressionConstants.TIMESPAN:

                    return ExitTimespan((Token)node);
                case (int)ExpressionConstants.DATETIME:

                    return ExitDatetime((Token)node);
                case (int)ExpressionConstants.IF:

                    return ExitIf((Token)node);
                case (int)ExpressionConstants.CAST:

                    return ExitCast((Token)node);
                case (int)ExpressionConstants.EXPRESSION:

                    return ExitExpression((Production)node);
                case (int)ExpressionConstants.XOR_EXPRESSION:

                    return ExitXorExpression((Production)node);
                case (int)ExpressionConstants.OR_EXPRESSION:

                    return ExitOrExpression((Production)node);
                case (int)ExpressionConstants.AND_EXPRESSION:

                    return ExitAndExpression((Production)node);
                case (int)ExpressionConstants.NOT_EXPRESSION:

                    return ExitNotExpression((Production)node);
                case (int)ExpressionConstants.IN_EXPRESSION:

                    return ExitInExpression((Production)node);
                case (int)ExpressionConstants.IN_TARGET_EXPRESSION:

                    return ExitInTargetExpression((Production)node);
                case (int)ExpressionConstants.IN_LIST_TARGET_EXPRESSION:

                    return ExitInListTargetExpression((Production)node);
                case (int)ExpressionConstants.COMPARE_EXPRESSION:

                    return ExitCompareExpression((Production)node);
                case (int)ExpressionConstants.SHIFT_EXPRESSION:

                    return ExitShiftExpression((Production)node);
                case (int)ExpressionConstants.ADDITIVE_EXPRESSION:

                    return ExitAdditiveExpression((Production)node);
                case (int)ExpressionConstants.MULTIPLICATIVE_EXPRESSION:

                    return ExitMultiplicativeExpression((Production)node);
                case (int)ExpressionConstants.POWER_EXPRESSION:

                    return ExitPowerExpression((Production)node);
                case (int)ExpressionConstants.NEGATE_EXPRESSION:

                    return ExitNegateExpression((Production)node);
                case (int)ExpressionConstants.MEMBER_EXPRESSION:

                    return ExitMemberExpression((Production)node);
                case (int)ExpressionConstants.MEMBER_ACCESS_EXPRESSION:

                    return ExitMemberAccessExpression((Production)node);
                case (int)ExpressionConstants.BASIC_EXPRESSION:

                    return ExitBasicExpression((Production)node);
                case (int)ExpressionConstants.MEMBER_FUNCTION_EXPRESSION:

                    return ExitMemberFunctionExpression((Production)node);
                case (int)ExpressionConstants.FIELD_PROPERTY_EXPRESSION:

                    return ExitFieldPropertyExpression((Production)node);
                case (int)ExpressionConstants.SPECIAL_FUNCTION_EXPRESSION:

                    return ExitSpecialFunctionExpression((Production)node);
                case (int)ExpressionConstants.IF_EXPRESSION:

                    return ExitIfExpression((Production)node);
                case (int)ExpressionConstants.CAST_EXPRESSION:

                    return ExitCastExpression((Production)node);
                case (int)ExpressionConstants.CAST_TYPE_EXPRESSION:

                    return ExitCastTypeExpression((Production)node);
                case (int)ExpressionConstants.INDEX_EXPRESSION:

                    return ExitIndexExpression((Production)node);
                case (int)ExpressionConstants.FUNCTION_CALL_EXPRESSION:

                    return ExitFunctionCallExpression((Production)node);
                case (int)ExpressionConstants.ARGUMENT_LIST:

                    return ExitArgumentList((Production)node);
                case (int)ExpressionConstants.LITERAL_EXPRESSION:

                    return ExitLiteralExpression((Production)node);
                case (int)ExpressionConstants.BOOLEAN_LITERAL_EXPRESSION:

                    return ExitBooleanLiteralExpression((Production)node);
                case (int)ExpressionConstants.EXPRESSION_GROUP:

                    return ExitExpressionGroup((Production)node);
            }
            return node;
        }

        /// <summary>
        /// Called when adding a child to a parse tree node.
        /// </summary>
        /// <param name="node"></param>
        /// <param name="child"></param>
        public override void Child(Production node, Node child)
        {
            switch (node.Id)
            {
                case (int)ExpressionConstants.EXPRESSION:
                    ChildExpression(node, child);

                    break;
                case (int)ExpressionConstants.XOR_EXPRESSION:
                    ChildXorExpression(node, child);

                    break;
                case (int)ExpressionConstants.OR_EXPRESSION:
                    ChildOrExpression(node, child);

                    break;
                case (int)ExpressionConstants.AND_EXPRESSION:
                    ChildAndExpression(node, child);

                    break;
                case (int)ExpressionConstants.NOT_EXPRESSION:
                    ChildNotExpression(node, child);

                    break;
                case (int)ExpressionConstants.IN_EXPRESSION:
                    ChildInExpression(node, child);

                    break;
                case (int)ExpressionConstants.IN_TARGET_EXPRESSION:
                    ChildInTargetExpression(node, child);

                    break;
                case (int)ExpressionConstants.IN_LIST_TARGET_EXPRESSION:
                    ChildInListTargetExpression(node, child);

                    break;
                case (int)ExpressionConstants.COMPARE_EXPRESSION:
                    ChildCompareExpression(node, child);

                    break;
                case (int)ExpressionConstants.SHIFT_EXPRESSION:
                    ChildShiftExpression(node, child);

                    break;
                case (int)ExpressionConstants.ADDITIVE_EXPRESSION:
                    ChildAdditiveExpression(node, child);

                    break;
                case (int)ExpressionConstants.MULTIPLICATIVE_EXPRESSION:
                    ChildMultiplicativeExpression(node, child);

                    break;
                case (int)ExpressionConstants.POWER_EXPRESSION:
                    ChildPowerExpression(node, child);

                    break;
                case (int)ExpressionConstants.NEGATE_EXPRESSION:
                    ChildNegateExpression(node, child);

                    break;
                case (int)ExpressionConstants.MEMBER_EXPRESSION:
                    ChildMemberExpression(node, child);

                    break;
                case (int)ExpressionConstants.MEMBER_ACCESS_EXPRESSION:
                    ChildMemberAccessExpression(node, child);

                    break;
                case (int)ExpressionConstants.BASIC_EXPRESSION:
                    ChildBasicExpression(node, child);

                    break;
                case (int)ExpressionConstants.MEMBER_FUNCTION_EXPRESSION:
                    ChildMemberFunctionExpression(node, child);

                    break;
                case (int)ExpressionConstants.FIELD_PROPERTY_EXPRESSION:
                    ChildFieldPropertyExpression(node, child);

                    break;
                case (int)ExpressionConstants.SPECIAL_FUNCTION_EXPRESSION:
                    ChildSpecialFunctionExpression(node, child);

                    break;
                case (int)ExpressionConstants.IF_EXPRESSION:
                    ChildIfExpression(node, child);

                    break;
                case (int)ExpressionConstants.CAST_EXPRESSION:
                    ChildCastExpression(node, child);

                    break;
                case (int)ExpressionConstants.CAST_TYPE_EXPRESSION:
                    ChildCastTypeExpression(node, child);

                    break;
                case (int)ExpressionConstants.INDEX_EXPRESSION:
                    ChildIndexExpression(node, child);

                    break;
                case (int)ExpressionConstants.FUNCTION_CALL_EXPRESSION:
                    ChildFunctionCallExpression(node, child);

                    break;
                case (int)ExpressionConstants.ARGUMENT_LIST:
                    ChildArgumentList(node, child);

                    break;
                case (int)ExpressionConstants.LITERAL_EXPRESSION:
                    ChildLiteralExpression(node, child);

                    break;
                case (int)ExpressionConstants.BOOLEAN_LITERAL_EXPRESSION:
                    ChildBooleanLiteralExpression(node, child);

                    break;
                case (int)ExpressionConstants.EXPRESSION_GROUP:
                    ChildExpressionGroup(node, child);

                    break;
            }
        }

        public virtual void EnterAdd(Token node)
        {
        }

        public virtual Node ExitAdd(Token node)
        {
            return node;
        }

        public virtual void EnterSub(Token node)
        {
        }

        
        public virtual Node ExitSub(Token node)
        {
            return node;
        }

        public virtual void EnterMul(Token node)
        {
        }

        public virtual Node ExitMul(Token node)
        {
            return node;
        }

        public virtual void EnterDiv(Token node)
        {
        }

        public virtual Node ExitDiv(Token node)
        {
            return node;
        }

        public virtual void EnterPower(Token node)
        {
        }

        public virtual Node ExitPower(Token node)
        {
            return node;
        }

        public virtual void EnterMod(Token node)
        {
        }

        public virtual Node ExitMod(Token node)
        {
            return node;
        }

        public virtual void EnterLeftParen(Token node)
        {
        }

        public virtual Node ExitLeftParen(Token node)
        {
            return node;
        }

        public virtual void EnterRightParen(Token node)
        {
        }

        public virtual Node ExitRightParen(Token node)
        {
            return node;
        }

        public virtual void EnterLeftBrace(Token node)
        {
        }

        public virtual Node ExitLeftBrace(Token node)
        {
            return node;
        }

        public virtual void EnterRightBrace(Token node)
        {
        }

        public virtual Node ExitRightBrace(Token node)
        {
            return node;
        }

        public virtual void EnterEq(Token node)
        {
        }

        public virtual Node ExitEq(Token node)
        {
            return node;
        }

        public virtual void EnterLt(Token node)
        {
        }

        public virtual Node ExitLt(Token node)
        {
            return node;
        }

        public virtual void EnterGt(Token node)
        {
        }

        public virtual Node ExitGt(Token node)
        {
            return node;
        }

        public virtual void EnterLte(Token node)
        {
        }

        public virtual Node ExitLte(Token node)
        {
            return node;
        }

        public virtual void EnterGte(Token node)
        {
        }

        public virtual Node ExitGte(Token node)
        {
            return node;
        }

        public virtual void EnterNe(Token node)
        {
        }

        public virtual Node ExitNe(Token node)
        {
            return node;
        }

        public virtual void EnterAnd(Token node)
        {
        }

        public virtual Node ExitAnd(Token node)
        {
            return node;
        }

        public virtual void EnterOr(Token node)
        {
        }

        public virtual Node ExitOr(Token node)
        {
            return node;
        }

        public virtual void EnterXor(Token node)
        {
        }

        public virtual Node ExitXor(Token node)
        {
            return node;
        }

        public virtual void EnterNot(Token node)
        {
        }

        public virtual Node ExitNot(Token node)
        {
            return node;
        }

        public virtual void EnterIn(Token node)
        {
        }

        public virtual Node ExitIn(Token node)
        {
            return node;
        }

        public virtual void EnterDot(Token node)
        {
        }

        public virtual Node ExitDot(Token node)
        {
            return node;
        }

        public virtual void EnterArgumentSeparator(Token node)
        {
        }

        public virtual Node ExitArgumentSeparator(Token node)
        {
            return node;
        }

        public virtual void EnterArrayBraces(Token node)
        {
        }

        public virtual Node ExitArrayBraces(Token node)
        {
            return node;
        }

        public virtual void EnterLeftShift(Token node)
        {
        }

        public virtual Node ExitLeftShift(Token node)
        {
            return node;
        }

        public virtual void EnterRightShift(Token node)
        {
        }

        public virtual Node ExitRightShift(Token node)
        {
            return node;
        }

        public virtual void EnterInteger(Token node)
        {
        }

        public virtual Node ExitInteger(Token node)
        {
            return node;
        }

        public virtual void EnterReal(Token node)
        {
        }

        public virtual Node ExitReal(Token node)
        {
            return node;
        }

        public virtual void EnterStringLiteral(Token node)
        {
        }

        public virtual Node ExitStringLiteral(Token node)
        {
            return node;
        }

        public virtual void EnterCharLiteral(Token node)
        {
        }

        public virtual Node ExitCharLiteral(Token node)
        {
            return node;
        }

        public virtual void EnterTrue(Token node)
        {
        }

        public virtual Node ExitTrue(Token node)
        {
            return node;
        }

        public virtual void EnterFalse(Token node)
        {
        }

        public virtual Node ExitFalse(Token node)
        {
            return node;
        }

        public virtual void EnterIdentifier(Token node)
        {
        }

        public virtual Node ExitIdentifier(Token node)
        {
            return node;
        }

        public virtual void EnterHexLiteral(Token node)
        {
        }

        public virtual Node ExitHexliteral(Token node)
        {
            return node;
        }

        public virtual void EnterNullLiteral(Token node)
        {
        }

        public virtual Node ExitNullLiteral(Token node)
        {
            return node;
        }

        public virtual void EnterTimespan(Token node)
        {
        }

        public virtual Node ExitTimespan(Token node)
        {
            return node;
        }

        public virtual void EnterDatetime(Token node)
        {
        }

        public virtual Node ExitDatetime(Token node)
        {
            return node;
        }

        public virtual void EnterIf(Token node)
        {
        }

        public virtual Node ExitIf(Token node)
        {
            return node;
        }

        public virtual void EnterCast(Token node)
        {
        }

        public virtual Node ExitCast(Token node)
        {
            return node;
        }

        public virtual void EnterExpression(Production node)
        {
        }

        public virtual Node ExitExpression(Production node)
        {
            return node;
        }

        public virtual void ChildExpression(Production node, Node child)
        {
            node.AddChild(child);
        }

        public virtual void EnterXorExpression(Production node)
        {
        }

        public virtual Node ExitXorExpression(Production node)
        {
            return node;
        }

        public virtual void ChildXorExpression(Production node, Node child)
        {
            node.AddChild(child);
        }

        public virtual void EnterOrExpression(Production node)
        {
        }

        public virtual Node ExitOrExpression(Production node)
        {
            return node;
        }

        public virtual void ChildOrExpression(Production node, Node child)
        {
            node.AddChild(child);
        }

        public virtual void EnterAndExpression(Production node)
        {
        }

        public virtual Node ExitAndExpression(Production node)
        {
            return node;
        }

        public virtual void ChildAndExpression(Production node, Node child)
        {
            node.AddChild(child);
        }

        public virtual void EnterNotExpression(Production node)
        {
        }

        public virtual Node ExitNotExpression(Production node)
        {
            return node;
        }

        public virtual void ChildNotExpression(Production node, Node child)
        {
            node.AddChild(child);
        }

        public virtual void EnterInExpression(Production node)
        {
        }

        public virtual Node ExitInExpression(Production node)
        {
            return node;
        }

        public virtual void ChildInExpression(Production node, Node child)
        {
            node.AddChild(child);
        }

        public virtual void EnterInTargetExpression(Production node)
        {
        }

        public virtual Node ExitInTargetExpression(Production node)
        {
            return node;
        }

        public virtual void ChildInTargetExpression(Production node, Node child)
        {
            node.AddChild(child);
        }

        public virtual void EnterInListTargetExpression(Production node)
        {
        }

        public virtual Node ExitInListTargetExpression(Production node)
        {
            return node;
        }

        public virtual void ChildInListTargetExpression(Production node, Node child)
        {
            node.AddChild(child);
        }

        public virtual void EnterCompareExpression(Production node)
        {
        }

        public virtual Node ExitCompareExpression(Production node)
        {
            return node;
        }

        public virtual void ChildCompareExpression(Production node, Node child)
        {
            node.AddChild(child);
        }

        public virtual void EnterShiftExpression(Production node)
        {
        }

        public virtual Node ExitShiftExpression(Production node)
        {
            return node;
        }

        public virtual void ChildShiftExpression(Production node, Node child)
        {
            node.AddChild(child);
        }

        public virtual void EnterAdditiveExpression(Production node)
        {
        }

        public virtual Node ExitAdditiveExpression(Production node)
        {
            return node;
        }

        public virtual void ChildAdditiveExpression(Production node, Node child)
        {
            node.AddChild(child);
        }

        public virtual void EnterMultiplicativeExpression(Production node)
        {
        }

        public virtual Node ExitMultiplicativeExpression(Production node)
        {
            return node;
        }

       
        public virtual void ChildMultiplicativeExpression(Production node, Node child)
        {
            node.AddChild(child);
        }

        public virtual void EnterPowerExpression(Production node)
        {
        }

        public virtual Node ExitPowerExpression(Production node)
        {
            return node;
        }

        public virtual void ChildPowerExpression(Production node, Node child)
        {
            node.AddChild(child);
        }

        public virtual void EnterNegateExpression(Production node)
        {
        }

        public virtual Node ExitNegateExpression(Production node)
        {
            return node;
        }

        public virtual void ChildNegateExpression(Production node, Node child)
        {
            node.AddChild(child);
        }

        public virtual void EnterMemberExpression(Production node)
        {
        }

        public virtual Node ExitMemberExpression(Production node)
        {
            return node;
        }

        public virtual void ChildMemberExpression(Production node, Node child)
        {
            node.AddChild(child);
        }

        public virtual void EnterMemberAccessExpression(Production node)
        {
        }

        public virtual Node ExitMemberAccessExpression(Production node)
        {
            return node;
        }

        public virtual void ChildMemberAccessExpression(Production node, Node child)
        {
            node.AddChild(child);
        }

        public virtual void EnterBasicExpression(Production node)
        {
        }

        public virtual Node ExitBasicExpression(Production node)
        {
            return node;
        }

        public virtual void ChildBasicExpression(Production node, Node child)
        {
            node.AddChild(child);
        }

        public virtual void EnterMemberFunctionExpression(Production node)
        {
        }

        public virtual Node ExitMemberFunctionExpression(Production node)
        {
            return node;
        }

        public virtual void ChildMemberFunctionExpression(Production node, Node child)
        {
            node.AddChild(child);
        }

        public virtual void EnterFieldPropertyExpression(Production node)
        {
        }

        public virtual Node ExitFieldPropertyExpression(Production node)
        {
            return node;
        }

        public virtual void ChildFieldPropertyExpression(Production node, Node child)
        {
            node.AddChild(child);
        }

        public virtual void EnterSpecialFunctionExpression(Production node)
        {
        }

        public virtual Node ExitSpecialFunctionExpression(Production node)
        {
            return node;
        }

        public virtual void ChildSpecialFunctionExpression(Production node, Node child)
        {
            node.AddChild(child);
        }

        public virtual void EnterIfExpression(Production node)
        {
        }

        public virtual Node ExitIfExpression(Production node)
        {
            return node;
        }

        public virtual void ChildIfExpression(Production node, Node child)
        {
            node.AddChild(child);
        }

        public virtual void EnterCastExpression(Production node)
        {
        }

        public virtual Node ExitCastExpression(Production node)
        {
            return node;
        }

        public virtual void ChildCastExpression(Production node, Node child)
        {
            node.AddChild(child);
        }

        public virtual void EnterCastTypeExpression(Production node)
        {
        }

        public virtual Node ExitCastTypeExpression(Production node)
        {
            return node;
        }

        public virtual void ChildCastTypeExpression(Production node, Node child)
        {
            node.AddChild(child);
        }

        public virtual void EnterIndexExpression(Production node)
        {
        }

        public virtual Node ExitIndexExpression(Production node)
        {
            return node;
        }

        public virtual void ChildIndexExpression(Production node, Node child)
        {
            node.AddChild(child);
        }

        public virtual void EnterFunctionCallExpression(Production node)
        {
        }

        public virtual Node ExitFunctionCallExpression(Production node)
        {
            return node;
        }

        public virtual void ChildFunctionCallExpression(Production node, Node child)
        {
            node.AddChild(child);
        }

        public virtual void EnterArgumentList(Production node)
        {
        }

        public virtual Node ExitArgumentList(Production node)
        {
            return node;
        }

        public virtual void ChildArgumentList(Production node, Node child)
        {
            node.AddChild(child);
        }

        public virtual void EnterLiteralExpression(Production node)
        {
        }

        public virtual Node ExitLiteralExpression(Production node)
        {
            return node;
        }

        public virtual void ChildLiteralExpression(Production node, Node child)
        {
            node.AddChild(child);
        }

        public virtual void EnterBooleanLiteralExpression(Production node)
        {
        }

        public virtual Node ExitBooleanLiteralExpression(Production node)
        {
            return node;
        }

        public virtual void ChildBooleanLiteralExpression(Production node, Node child)
        {
            node.AddChild(child);
        }

        public virtual void EnterExpressionGroup(Production node)
        {
        }

        public virtual Node ExitExpressionGroup(Production node)
        {
            return node;
        }

        public virtual void ChildExpressionGroup(Production node, Node child)
        {
            node.AddChild(child);
        }
    }
}
