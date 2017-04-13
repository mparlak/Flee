
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using Flee.Parsing;
using Flee.Parsing.grammatica_1._5.alpha2.PerCederberg.Grammatica.Runtime;
using Flee.PublicTypes;

namespace Flee.CalcEngine.InternalTypes
{
    internal class IdentifierAnalyzer : Analyzer
    {

        private readonly IDictionary<int, string> _myIdentifiers;
        private int _myMemberExpressionCount;

        private bool _myInFieldPropertyExpression;
        public IdentifierAnalyzer()
        {
            _myIdentifiers = new Dictionary<int, string>();
        }

        public override Node Exit(Node node)
        {
            switch (node.Id)
            {
                case (int)ExpressionConstants.IDENTIFIER:
                    this.ExitIdentifier((Token)node);
                    break;
                case (int)ExpressionConstants.FIELD_PROPERTY_EXPRESSION:
                    this.ExitFieldPropertyExpression();
                    break;
            }

            return node;
        }

        public override void Enter(Node node)
        {
            switch (node.Id)
            {
                case (int)ExpressionConstants.MEMBER_EXPRESSION:
                    this.EnterMemberExpression();
                    break;
                case (int)ExpressionConstants.FIELD_PROPERTY_EXPRESSION:
                    this.EnterFieldPropertyExpression();
                    break;
            }
        }

        private void ExitIdentifier(Token node)
        {
            if (_myInFieldPropertyExpression == false)
            {
                return;
            }

            if (_myIdentifiers.ContainsKey(_myMemberExpressionCount) == false)
            {
                _myIdentifiers.Add(_myMemberExpressionCount, node.Image);
            }
        }

        private void EnterMemberExpression()
        {
            _myMemberExpressionCount += 1;
        }

        private void EnterFieldPropertyExpression()
        {
            _myInFieldPropertyExpression = true;
        }

        private void ExitFieldPropertyExpression()
        {
            _myInFieldPropertyExpression = false;
        }

        public override void Reset()
        {
            _myIdentifiers.Clear();
            _myMemberExpressionCount = -1;
        }

        public ICollection<string> GetIdentifiers(ExpressionContext context)
        {
            Dictionary<string, object> dict = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
            ExpressionImports ei = context.Imports;

            foreach (string identifier in _myIdentifiers.Values)
            {
                // Skip names registered as namespaces
                if (ei.HasNamespace(identifier) == true)
                {
                    continue;
                }
                else if (context.Variables.ContainsKey(identifier) == true)
                {
                    // Identifier is a variable
                    continue;
                }

                // Get only the unique values
                dict[identifier] = null;
            }

            return dict.Keys;
        }
    }
}
