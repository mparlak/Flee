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

        public string Name
        {
            get { return _myName; }
        }

        public abstract Type ResultType { get; }
        public abstract object ResultAsObject { get; set; }

        public IDynamicExpression Expression
        {
            get { return MyExpression; }
        }
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

        public T Result
        {
            get { return MyResult; }
        }

        public override System.Type ResultType
        {
            get { return typeof(T); }
        }

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

        private string MyName;

        private object MyResult;

        internal NodeEventArgs()
        {
        }

        internal void SetData(string name, object result)
        {
            MyName = name;
            MyResult = result;
        }

        public string Name
        {
            get { return MyName; }
        }

        public object Result
        {
            get { return MyResult; }
        }
    }

}

