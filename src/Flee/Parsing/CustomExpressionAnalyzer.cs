using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Text.RegularExpressions;
using Flee.ExpressionElements;
using Flee.ExpressionElements.Base;
using Flee.ExpressionElements.Base.Literals;
using Flee.ExpressionElements.Literals;
using Flee.ExpressionElements.Literals.Integral;
using Flee.ExpressionElements.LogicalBitwise;
using Flee.ExpressionElements.MemberElements;
using Flee.InternalTypes;
using Flee.Parsing.grammatica_1._5.alpha2.PerCederberg.Grammatica.Runtime;
using Flee.PublicTypes;

namespace Flee.Parsing
{
    internal class FleeExpressionAnalyzer : ExpressionAnalyzer
    {

        private IServiceProvider _myServices;
        private readonly Regex _myUnicodeEscapeRegex;
        private readonly Regex _myRegularEscapeRegex;

        private bool _myInUnaryNegate;
        internal FleeExpressionAnalyzer()
        {
            _myUnicodeEscapeRegex = new Regex("\\\\u[0-9a-f]{4}", RegexOptions.IgnoreCase);
            _myRegularEscapeRegex = new Regex("\\\\[\\\\\"'trn]", RegexOptions.IgnoreCase);
        }

        public void SetServices(IServiceProvider services)
        {
            _myServices = services;
        }

        public override void Reset()
        {
            _myServices = null;
        }

        public override Node ExitExpression(Production node)
        {
            this.AddFirstChildValue(node);
            return node;
        }

        public override Node ExitExpressionGroup(Production node)
        {
            node.AddValues(this.GetChildValues(node));
            return node;
        }

        public override Node ExitXorExpression(Production node)
        {
            this.AddBinaryOp(node, typeof(XorElement));
            return node;
        }

        public override Node ExitOrExpression(Production node)
        {
            this.AddBinaryOp(node, typeof(AndOrElement));
            return node;
        }

        public override Node ExitAndExpression(Production node)
        {
            this.AddBinaryOp(node, typeof(AndOrElement));
            return node;
        }

        public override Node ExitNotExpression(Production node)
        {
            this.AddUnaryOp(node, typeof(NotElement));
            return node;
        }

        public override Node ExitCompareExpression(Production node)
        {
            this.AddBinaryOp(node, typeof(CompareElement));
            return node;
        }

        public override Node ExitShiftExpression(Production node)
        {
            this.AddBinaryOp(node, typeof(ShiftElement));
            return node;
        }

        public override Node ExitAdditiveExpression(Production node)
        {
            this.AddBinaryOp(node, typeof(ArithmeticElement));
            return node;
        }

        public override Node ExitMultiplicativeExpression(Production node)
        {
            this.AddBinaryOp(node, typeof(ArithmeticElement));
            return node;
        }

        public override Node ExitPowerExpression(Production node)
        {
            this.AddBinaryOp(node, typeof(ArithmeticElement));
            return node;
        }

        // Try to fold a negated constant int32.  We have to do this so that parsing int32.MinValue will work
        public override Node ExitNegateExpression(Production node)
        {
            IList childValues = this.GetChildValues(node);

            // Get last child
            ExpressionElement childElement = (ExpressionElement)childValues[childValues.Count - 1];

            // Is it an signed integer constant?
            if (object.ReferenceEquals(childElement.GetType(), typeof(Int32LiteralElement)) & childValues.Count == 2)
            {
                ((Int32LiteralElement)childElement).Negate();
                // Add it directly instead of the negate element since it will already be negated
                node.AddValue(childElement);
            }
            else if (object.ReferenceEquals(childElement.GetType(), typeof(Int64LiteralElement)) & childValues.Count == 2)
            {
                ((Int64LiteralElement)childElement).Negate();
                // Add it directly instead of the negate element since it will already be negated
                node.AddValue(childElement);
            }
            else
            {
                // No so just add a regular negate
                this.AddUnaryOp(node, typeof(NegateElement));
            }

            return node;
        }

        public override Node ExitMemberExpression(Production node)
        {
            IList childValues = this.GetChildValues(node);
            object first = childValues[0];

            if (childValues.Count == 1 && !(first is MemberElement))
            {
                node.AddValue(first);
            }
            else
            {
                InvocationListElement list = new InvocationListElement(childValues, _myServices);
                node.AddValue(list);
            }

            return node;
        }

        public override Node ExitIndexExpression(Production node)
        {
            IList childValues = this.GetChildValues(node);
            ArgumentList args = new ArgumentList(childValues);
            IndexerElement e = new IndexerElement(args);
            node.AddValue(e);
            return node;
        }

        public override Node ExitMemberAccessExpression(Production node)
        {
            node.AddValue(node.GetChildAt(1).GetValue(0));
            return node;
        }

        public override Node ExitSpecialFunctionExpression(Production node)
        {
            this.AddFirstChildValue(node);
            return node;
        }

        public override Node ExitIfExpression(Production node)
        {
            IList childValues = this.GetChildValues(node);
            ConditionalElement op = new ConditionalElement((ExpressionElement)childValues[0], (ExpressionElement)childValues[1], (ExpressionElement)childValues[2]);
            node.AddValue(op);
            return node;
        }

        public override Node ExitInExpression(Production node)
        {
            IList childValues = this.GetChildValues(node);

            if (childValues.Count == 1)
            {
                this.AddFirstChildValue(node);
                return node;
            }

            ExpressionElement operand = (ExpressionElement)childValues[0];
            childValues.RemoveAt(0);

            object second = childValues[0];
            InElement op = default(InElement);

            if ((second) is IList)
            {
                op = new InElement(operand, (IList)second);
            }
            else
            {
                InvocationListElement il = new InvocationListElement(childValues, _myServices);
                op = new InElement(operand, il);
            }

            node.AddValue(op);
            return node;
        }

        public override Node ExitInTargetExpression(Production node)
        {
            this.AddFirstChildValue(node);
            return node;
        }

        public override Node ExitInListTargetExpression(Production node)
        {
            IList childValues = this.GetChildValues(node);
            node.AddValue(childValues);
            return node;
        }

        public override Node ExitCastExpression(Production node)
        {
            IList childValues = this.GetChildValues(node);
            string[] destTypeParts = (string[])childValues[1];
            bool isArray = (bool)childValues[2];
            CastElement op = new CastElement((ExpressionElement)childValues[0], destTypeParts, isArray, _myServices);
            node.AddValue(op);
            return node;
        }

        public override Node ExitCastTypeExpression(Production node)
        {
            IList childValues = this.GetChildValues(node);
            List<string> parts = new List<string>();

            foreach (string part in childValues)
            {
                parts.Add(part);
            }

            bool isArray = false;

            if (parts[parts.Count - 1] == "[]")
            {
                isArray = true;
                parts.RemoveAt(parts.Count - 1);
            }

            node.AddValue(parts.ToArray());
            node.AddValue(isArray);
            return node;
        }

        public override Node ExitMemberFunctionExpression(Production node)
        {
            this.AddFirstChildValue(node);
            return node;
        }

        public override Node ExitFieldPropertyExpression(Production node)
        {
            //string name = ((Token)node.GetChildAt(0))?.Image;
            string name = node.GetChildAt(0).GetValue(0).ToString();
            IdentifierElement elem = new IdentifierElement(name);
            node.AddValue(elem);
            return node;
        }

        public override Node ExitFunctionCallExpression(Production node)
        {
            IList childValues = this.GetChildValues(node);
            string name = (string)childValues[0];
            childValues.RemoveAt(0);
            ArgumentList args = new ArgumentList(childValues);
            FunctionCallElement funcCall = new FunctionCallElement(name, args);
            node.AddValue(funcCall);
            return node;
        }

        public override Node ExitArgumentList(Production node)
        {
            IList childValues = this.GetChildValues(node);
            node.AddValues((ArrayList)childValues);
            return node;
        }

        public override Node ExitBasicExpression(Production node)
        {
            this.AddFirstChildValue(node);
            return node;
        }

        public override Node ExitLiteralExpression(Production node)
        {
            this.AddFirstChildValue(node);
            return node;
        }

        private void AddFirstChildValue(Production node)
        {
            node.AddValue(this.GetChildAt(node, 0).Values[0]);
        }

        private void AddUnaryOp(Production node, Type elementType)
        {
            IList childValues = this.GetChildValues(node);

            if (childValues.Count == 2)
            {
                UnaryElement element = (UnaryElement)Activator.CreateInstance(elementType);
                element.SetChild((ExpressionElement)childValues[1]);
                node.AddValue(element);
            }
            else
            {
                node.AddValue(childValues[0]);
            }
        }

        private void AddBinaryOp(Production node, Type elementType)
        {
            IList childValues = this.GetChildValues(node);

            if (childValues.Count > 1)
            {
                BinaryExpressionElement e = BinaryExpressionElement.CreateElement(childValues, elementType);
                node.AddValue(e);
            }
            else if (childValues.Count == 1)
            {
                node.AddValue(childValues[0]);
            }
            else
            {
                Debug.Assert(false, "wrong number of chilren");
            }
        }

        public override Node ExitReal(Token node)
        {
            string image = node.Image;
            LiteralElement element = RealLiteralElement.Create(image, _myServices);

            node.AddValue(element);
            return node;
        }

        public override Node ExitInteger(Token node)
        {
            LiteralElement element = IntegralLiteralElement.Create(node.Image, false, _myInUnaryNegate, _myServices);
            node.AddValue(element);
            return node;
        }

        public override Node ExitHexliteral(Token node)
        {
            LiteralElement element = IntegralLiteralElement.Create(node.Image, true, _myInUnaryNegate, _myServices);
            node.AddValue(element);
            return node;
        }

        public override Node ExitBooleanLiteralExpression(Production node)
        {
            this.AddFirstChildValue(node);
            return node;
        }

        public override Node ExitTrue(Token node)
        {
            node.AddValue(new BooleanLiteralElement(true));
            return node;
        }

        public override Node ExitFalse(Token node)
        {
            node.AddValue(new BooleanLiteralElement(false));
            return node;
        }

        public override Node ExitStringLiteral(Token node)
        {
            string s = this.DoEscapes(node.Image);
            StringLiteralElement element = new StringLiteralElement(s);
            node.AddValue(element);
            return node;
        }

        public override Node ExitCharLiteral(Token node)
        {
            string s = this.DoEscapes(node.Image);
            node.AddValue(new CharLiteralElement(s[0]));
            return node;
        }

        public override Node ExitDatetime(Token node)
        {
            ExpressionContext context = (ExpressionContext)_myServices.GetService(typeof(ExpressionContext));
            string image = node.Image.Substring(1, node.Image.Length - 2);
            DateTimeLiteralElement element = new DateTimeLiteralElement(image, context);
            node.AddValue(element);
            return node;
        }

        public override Node ExitTimespan(Token node)
        {
            string image = node.Image.Substring(2, node.Image.Length - 3);
            TimeSpanLiteralElement element = new TimeSpanLiteralElement(image);
            node.AddValue(element);
            return node;
        }

        private string DoEscapes(string image)
        {
            // Remove outer quotes
            image = image.Substring(1, image.Length - 2);
            image = _myUnicodeEscapeRegex.Replace(image, UnicodeEscapeMatcher);
            image = _myRegularEscapeRegex.Replace(image, RegularEscapeMatcher);
            return image;
        }

        private string RegularEscapeMatcher(Match m)
        {
            string s = m.Value;
            // Remove leading \
            s = s.Remove(0, 1);

            switch (s)
            {
                case "\\":
                case "\"":
                case "'":
                    return s;
                case "t":
                case "T":
                    return Convert.ToChar(9).ToString();
                case "n":
                case "N":
                    return Convert.ToChar(10).ToString();
                case "r":
                case "R":
                    return Convert.ToChar(13).ToString();
                default:
                    Debug.Assert(false, "Unrecognized escape sequence");
                    return null;
            }
        }

        private string UnicodeEscapeMatcher(Match m)
        {
            string s = m.Value;
            // Remove \u
            s = s.Remove(0, 2);
            int code = int.Parse(s, NumberStyles.AllowHexSpecifier);
            char c = Convert.ToChar(code);
            return c.ToString();
        }

        public override Node ExitIdentifier(Token node)
        {
            node.AddValue(node.Image);
            return node;
        }

        public override Node ExitNullLiteral(Token node)
        {
            node.AddValue(new NullLiteralElement());
            return node;
        }

        public override Node ExitArrayBraces(Token node)
        {
            node.AddValue("[]");
            return node;
        }

        public override Node ExitAdd(Token node)
        {
            node.AddValue(BinaryArithmeticOperation.Add);
            return node;
        }

        public override Node ExitSub(Token node)
        {
            node.AddValue(BinaryArithmeticOperation.Subtract);
            return node;
        }

        public override Node ExitMul(Token node)
        {
            node.AddValue(BinaryArithmeticOperation.Multiply);
            return node;
        }

        public override Node ExitDiv(Token node)
        {
            node.AddValue(BinaryArithmeticOperation.Divide);
            return node;
        }

        public override Node ExitMod(Token node)
        {
            node.AddValue(BinaryArithmeticOperation.Mod);
            return node;
        }

        public override Node ExitPower(Token node)
        {
            node.AddValue(BinaryArithmeticOperation.Power);
            return node;
        }

        public override Node ExitEq(Token node)
        {
            node.AddValue(LogicalCompareOperation.Equal);
            return node;
        }

        public override Node ExitNe(Token node)
        {
            node.AddValue(LogicalCompareOperation.NotEqual);
            return node;
        }

        public override Node ExitLt(Token node)
        {
            node.AddValue(LogicalCompareOperation.LessThan);
            return node;
        }

        public override Node ExitGt(Token node)
        {
            node.AddValue(LogicalCompareOperation.GreaterThan);
            return node;
        }

        public override Node ExitLte(Token node)
        {
            node.AddValue(LogicalCompareOperation.LessThanOrEqual);
            return node;
        }

        public override Node ExitGte(Token node)
        {
            node.AddValue(LogicalCompareOperation.GreaterThanOrEqual);
            return node;
        }

        public override Node ExitAnd(Token node)
        {
            node.AddValue(AndOrOperation.And);
            return node;
        }

        public override Node ExitOr(Token node)
        {
            node.AddValue(AndOrOperation.Or);
            return node;
        }

        public override Node ExitXor(Token node)
        {
            node.AddValue("Xor");
            return node;
        }

        public override Node ExitNot(Token node)
        {
            node.AddValue(string.Empty);
            return node;
        }

        public override Node ExitLeftShift(Token node)
        {
            node.AddValue(ShiftOperation.LeftShift);
            return node;
        }

        public override Node ExitRightShift(Token node)
        {
            node.AddValue(ShiftOperation.RightShift);
            return node;
        }

        public override void Child(Production node, Node child)
        {
            base.Child(node, child);
            _myInUnaryNegate = node.Id == (int)ExpressionConstants.NEGATE_EXPRESSION & child.Id == (int)ExpressionConstants.SUB;
        }
    }
}
