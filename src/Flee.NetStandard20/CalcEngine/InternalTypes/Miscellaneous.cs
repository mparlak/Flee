using System;
using System.Collections.Generic;
using Flee.PublicTypes;

namespace Flee.CalcEngine.InternalTypes
{
    internal class PairEqualityComparer : EqualityComparer<ExpressionResultPair>
    {
        public override bool Equals(ExpressionResultPair x, ExpressionResultPair y)
        {
            return string.Equals(x.Name, y.Name, StringComparison.OrdinalIgnoreCase);
        }

        public override int GetHashCode(ExpressionResultPair obj)
        {
            return StringComparer.OrdinalIgnoreCase.GetHashCode(obj.Name);
        }
    }

    internal abstract class ExpressionResultPair
    {

        private string _myName;

        protected IDynamicExpression MyExpression;

        protected ExpressionResultPair()
        {
        }

        public abstract void Recalculate();

        public void SetExpression(IDynamicExpression e)
        {
            MyExpression = e;
        }

        public void SetName(string name)
        {
            _myName = name;
        }

        public override string ToString()
        {
            return _myName;
        }

        public string Name => _myName;

        public abstract Type ResultType { get; }
        public abstract object ResultAsObject { get; set; }

        public IDynamicExpression Expression => MyExpression;
    }

    internal class GenericExpressionResultPair<T> : ExpressionResultPair
    {
        public T MyResult;
        public GenericExpressionResultPair()
        {
        }

        public override void Recalculate()
        {
            MyResult = (T)MyExpression.Evaluate();
        }

        public T Result => MyResult;

        public override System.Type ResultType => typeof(T);

        public override object ResultAsObject
        {
            get { return MyResult; }
            set { MyResult = (T)value; }
        }
    }

    internal class BatchLoadInfo
    {
        public string Name;
        public string ExpressionText;

        public ExpressionContext Context;
        public BatchLoadInfo(string name, string text, ExpressionContext context)
        {
            this.Name = name;
            this.ExpressionText = text;
            this.Context = context;
        }
    }

    public sealed class NodeEventArgs : EventArgs
    {

        private string _myName;

        private object _myResult;

        internal NodeEventArgs()
        {
        }

        internal void SetData(string name, object result)
        {
            _myName = name;
            _myResult = result;
        }

        public string Name => _myName;

        public object Result => _myResult;
    }

}

