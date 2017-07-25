using System;
using System.Collections.Generic;
using System.IO;
using Flee.CalcEngine.InternalTypes;
using Flee.PublicTypes;

namespace Flee.CalcEngine.PublicTypes
{
    public class SimpleCalcEngine
    {

        #region "Fields"

        private readonly IDictionary<string, IExpression> _myExpressions;

        private ExpressionContext _myContext;
        #endregion

        #region "Constructor"

        public SimpleCalcEngine()
        {
            _myExpressions = new Dictionary<string, IExpression>(StringComparer.OrdinalIgnoreCase);
            _myContext = new ExpressionContext();
        }

        #endregion

        #region "Methods - Private"

        private void AddCompiledExpression(string expressionName, IExpression expression)
        {
            if (_myExpressions.ContainsKey(expressionName) == true)
            {
                throw new InvalidOperationException($"The calc engine already contains an expression named '{expressionName}'");
            }
            else
            {
                _myExpressions.Add(expressionName, expression);
            }
        }

        private ExpressionContext ParseAndLink(string expressionName, string expression)
        {
            IdentifierAnalyzer analyzer = Context.ParseIdentifiers(expression);

            ExpressionContext context2 = _myContext.CloneInternal(true);
            this.LinkExpression(expressionName, context2, analyzer);

            // Tell the expression not to clone the context since it's already been cloned
            context2.NoClone = true;

            // Clear our context's variables
            _myContext.Variables.Clear();

            return context2;
        }

        private void LinkExpression(string expressionName, ExpressionContext context, IdentifierAnalyzer analyzer)
        {
            foreach (string identifier in analyzer.GetIdentifiers(context))
            {
                this.LinkIdentifier(identifier, expressionName, context);
            }
        }

        private void LinkIdentifier(string identifier, string expressionName, ExpressionContext context)
        {
            IExpression child = null;

            if (_myExpressions.TryGetValue(identifier, out child) == false)
            {
                string msg = $"Expression '{expressionName}' references unknown name '{identifier}'";
                throw new InvalidOperationException(msg);
            }

            context.Variables.Add(identifier, child);
        }

        #endregion

        #region "Methods - Public"

        public void AddDynamic(string expressionName, string expression)
        {
            ExpressionContext linkedContext = this.ParseAndLink(expressionName, expression);
            IExpression e = linkedContext.CompileDynamic(expression);
            this.AddCompiledExpression(expressionName, e);
        }

        public void AddGeneric<T>(string expressionName, string expression)
        {
            ExpressionContext linkedContext = this.ParseAndLink(expressionName, expression);
            IExpression e = linkedContext.CompileGeneric<T>(expression);
            this.AddCompiledExpression(expressionName, e);
        }

        public void Clear()
        {
            _myExpressions.Clear();
        }

        #endregion

        #region "Properties - Public"
        public IExpression this[string name]
        {
            get
            {
                IExpression e = null;
                _myExpressions.TryGetValue(name, out e);
                return e;
            }
        }

        public ExpressionContext Context
        {
            get { return _myContext; }
            set { _myContext = value; }
        }
        #endregion
    }

}
