using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

namespace Flee.Parsing.grammatica_1._5.alpha2.PerCederberg.Grammatica.Runtime
{
    [Obsolete("Creates a new parse tree analyzer.")]
    internal class Analyzer
    {
        public Analyzer()
        {
        }

        /// <summary>
        /// Resets this analyzer when the parser is reset for another
        ///input stream.The default implementation of this method does
        /// nothing.
        /// </summary>
        public virtual void Reset()
        {
            // Default implementation does nothing
        }

        public Node Analyze(Node node)
        {
            ParserLogException log = new ParserLogException();

            node = Analyze(node, log);
            if (log.Count > 0)
            {
                throw log;
            }
            return node;
        }

        private Node Analyze(Node node, ParserLogException log)
        {
            var errorCount = log.Count;
            if (node is Production)
            {
                var prod = (Production)node;
                prod = NewProduction(prod.Pattern);
                try
                {
                    Enter(prod);
                }
                catch (ParseException e)
                {
                    log.AddError(e);
                }
                for (int i = 0; i < node.Count; i++)
                {
                    try
                    {
                        Child(prod, Analyze(node[i], log));
                    }
                    catch (ParseException e)
                    {
                        log.AddError(e);
                    }
                }
                try
                {
                    return Exit(prod);
                }
                catch (ParseException e)
                {
                    if (errorCount == log.Count)
                    {
                        log.AddError(e);
                    }
                }
            }
            else
            {
                node.Values.Clear();
                try
                {
                    Enter(node);
                }
                catch (ParseException e)
                {
                    log.AddError(e);
                }
                try
                {
                    return Exit(node);
                }
                catch (ParseException e)
                {
                    if (errorCount == log.Count)
                    {
                        log.AddError(e);
                    }
                }
            }
            return null;
        }

        public virtual Production NewProduction(ProductionPattern pattern)
        {
            return new Production(pattern);
        }

        public virtual void Enter(Node node)
        {
        }

        public virtual Node Exit(Node node)
        {
            return node;
        }

        public virtual void Child(Production node, Node child)
        {
            node.AddChild(child);
        }

        protected Node GetChildAt(Node node, int pos)
        {
            if (node == null)
            {
                throw new ParseException(
                    ParseException.ErrorType.INTERNAL,
                    "attempt to read 'null' parse tree node",
                    -1,
                    -1);
            }
            var child = node[pos];
            if (child == null)
            {
                throw new ParseException(
                    ParseException.ErrorType.INTERNAL,
                    "node '" + node.Name + "' has no child at " +
                    "position " + pos,
                    node.StartLine,
                    node.StartColumn);
            }
            return child;
        }

        protected Node GetChildWithId(Node node, int id)
        {
            if (node == null)
            {
                throw new ParseException(
                    ParseException.ErrorType.INTERNAL,
                    "attempt to read 'null' parse tree node",
                    -1,
                    -1);
            }
            for (int i = 0; i < node.Count; i++)
            {
                var child = node[i];
                if (child != null && child.Id == id)
                {
                    return child;
                }
            }
            throw new ParseException(
                ParseException.ErrorType.INTERNAL,
                "node '" + node.Name + "' has no child with id " + id,
                node.StartLine,
                node.StartColumn);
        }

        protected object GetValue(Node node, int pos)
        {
            if (node == null)
            {
                throw new ParseException(
                    ParseException.ErrorType.INTERNAL,
                    "attempt to read 'null' parse tree node",
                    -1,
                    -1);
            }
            var value = node.Values[pos];
            if (value == null)
            {
                throw new ParseException(
                    ParseException.ErrorType.INTERNAL,
                    "node '" + node.Name + "' has no value at " +
                    "position " + pos,
                    node.StartLine,
                    node.StartColumn);
            }
            return value;
        }

        protected int GetIntValue(Node node, int pos)
        {
            var value = GetValue(node, pos);
            if (value is int)
            {
                return (int)value;
            }
            else
            {
                throw new ParseException(
                    ParseException.ErrorType.INTERNAL,
                    "node '" + node.Name + "' has no integer value " +
                    "at position " + pos,
                    node.StartLine,
                    node.StartColumn);
            }
        }
       
        protected string GetStringValue(Node node, int pos)
        {
            var value = GetValue(node, pos);
            if (value is string)
            {
                return (string)value;
            }
            else
            {
                throw new ParseException(
                    ParseException.ErrorType.INTERNAL,
                    "node '" + node.Name + "' has no string value " +
                    "at position " + pos,
                    node.StartLine,
                    node.StartColumn);
            }
        }

        protected ArrayList GetChildValues(Node node)
        {
            ArrayList result = new ArrayList();

            for (int i = 0; i < node.Count; i++)
            {
                var child = node[i];
                var values = child.Values;
                if (values != null)
                {
                    result.AddRange(values);
                }
            }
            return result;
        }
    }
}
