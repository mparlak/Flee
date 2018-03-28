using System;
using System.Collections.Generic;
using System.Text;

namespace Flee.PublicTypes
{
    public interface IExpression
    {
        IExpression Clone();
        string Text { get; }
        ExpressionInfo Info { get; }
        ExpressionContext Context { get; }
        object Owner { get; set; }
    }

    public interface IDynamicExpression : IExpression
    {
        object Evaluate();
    }

    public interface IGenericExpression<T> : IExpression
    {
        T Evaluate();
    }

    public sealed class ExpressionInfo
    {


        private readonly IDictionary<string, object> _myData;
        internal ExpressionInfo()
        {
            _myData = new Dictionary<string, object>
            {
                {"ReferencedVariables", new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)}
            };
        }

        internal void AddReferencedVariable(string name)
        {
            IDictionary<string, string> dict = (IDictionary<string, string>)_myData["ReferencedVariables"];
            dict[name] = name;
        }

        public string[] GetReferencedVariables()
        {
            IDictionary<string, string> dict = (IDictionary<string, string>)_myData["ReferencedVariables"];
            string[] arr = new string[dict.Count];
            dict.Keys.CopyTo(arr, 0);
            return arr;
        }
    }

    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Method | AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
    public sealed class ExpressionOwnerMemberAccessAttribute : Attribute
    {


        private readonly bool _myAllowAccess;
        public ExpressionOwnerMemberAccessAttribute(bool allowAccess)
        {
            _myAllowAccess = allowAccess;
        }

        internal bool AllowAccess => _myAllowAccess;
    }

    public class ResolveVariableTypeEventArgs : EventArgs
    {
        private readonly string _myName;
        private Type _myType;
        internal ResolveVariableTypeEventArgs(string name)
        {
            this._myName = name;
        }

        public string VariableName => _myName;

        public Type VariableType
        {
            get { return _myType; }
            set { _myType = value; }
        }
    }

    public class ResolveVariableValueEventArgs : EventArgs
    {
        private readonly string _myName;
        private readonly Type _myType;

        private object MyValue;
        internal ResolveVariableValueEventArgs(string name, Type t)
        {
            _myName = name;
            _myType = t;
        }

        public string VariableName
        {
            get { return _myName; }
        }

        public Type VariableType
        {
            get { return _myType; }
        }

        public object VariableValue
        {
            get { return MyValue; }
            set { MyValue = value; }
        }
    }

    public class ResolveFunctionEventArgs : EventArgs
    {

        private readonly string MyName;
        private readonly Type[] MyArgumentTypes;

        private Type _myReturnType;
        internal ResolveFunctionEventArgs(string name, Type[] argumentTypes)
        {
            MyName = name;
            MyArgumentTypes = argumentTypes;
        }

        public string FunctionName
        {
            get { return MyName; }
        }

        public Type[] ArgumentTypes
        {
            get { return MyArgumentTypes; }
        }

        public Type ReturnType
        {
            get { return _myReturnType; }
            set { _myReturnType = value; }
        }
    }

    public class InvokeFunctionEventArgs : EventArgs
    {

        private readonly string _myName;
        private readonly object[] _myArguments;

        private object _myFunctionResult;
        internal InvokeFunctionEventArgs(string name, object[] arguments)
        {
            _myName = name;
            _myArguments = arguments;
        }

        public string FunctionName
        {
            get { return _myName; }
        }

        public object[] Arguments
        {
            get { return _myArguments; }
        }

        public object Result
        {
            get { return _myFunctionResult; }
            set { _myFunctionResult = value; }
        }
    }

    public enum RealLiteralDataType
    {
        Single,
        Double,
        Decimal
    }
}
