
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using Flee.PublicTypes;

namespace Flee.CalcEngine.PublicTypes
{

    public class CircularReferenceException : System.Exception
    {
        private readonly string _myCircularReferenceSource;

        internal CircularReferenceException()
        {
        }

        internal CircularReferenceException(string circularReferenceSource)
        {
            _myCircularReferenceSource = circularReferenceSource;
        }

        public override string Message
        {
            get
            {
                if (_myCircularReferenceSource == null)
                {
                    return "Circular reference detected in calculation engine";
                }
                else
                {
                    return $"Circular reference detected in calculation engine at '{_myCircularReferenceSource}'";
                }
            }
        }
    }

    public class BatchLoadCompileException : Exception
    {

        private readonly string _myAtomName;

        private readonly string _myExpressionText;
        internal BatchLoadCompileException(string atomName, string expressionText, ExpressionCompileException innerException) : base(
            $"Batch Load: The expression for atom '${atomName}' could not be compiled", innerException)
        {
            _myAtomName = atomName;
            _myExpressionText = expressionText;
        }

        public string AtomName => _myAtomName;

        public string ExpressionText => _myExpressionText;
    }

}

