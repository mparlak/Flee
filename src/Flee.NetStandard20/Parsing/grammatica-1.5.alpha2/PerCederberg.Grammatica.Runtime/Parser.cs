using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace Flee.Parsing.grammatica_1._5.alpha2.PerCederberg.Grammatica.Runtime
{

    [Obsolete(" A base parser class. This class provides the standard parser interface, as well as token handling.")]
    internal abstract class Parser
    {
        private bool _initialized;
        private readonly Tokenizer _tokenizer;
        private Analyzer _analyzer;
        private readonly ArrayList _patterns = new ArrayList();
        private readonly Hashtable _patternIds = new Hashtable();
        private readonly ArrayList _tokens = new ArrayList();
        private ParserLogException _errorLog = new ParserLogException();
        private int _errorRecovery = -1;

        /// <summary>
        /// Creates a new parser.
        /// </summary>
        /// <param name="input"></param>
        internal Parser(TextReader input) : this(input, null)
        {
        }

        /// <summary>
        /// Creates a new parser.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="analyzer"></param>
        internal Parser(TextReader input, Analyzer analyzer)
        {
            _tokenizer = NewTokenizer(input);
            this._analyzer = analyzer ?? NewAnalyzer();
        }

        /**
         * Creates a new parser.
         *
         * @param tokenizer       the tokenizer to use
         */
        internal Parser(Tokenizer tokenizer) : this(tokenizer, null)
        {
        }

        internal Parser(Tokenizer tokenizer, Analyzer analyzer)
        {
            this._tokenizer = tokenizer;
            this._analyzer = analyzer ?? NewAnalyzer();
        }

        protected virtual Tokenizer NewTokenizer(TextReader input)
        {
            // TODO: This method should really be abstract, but it isn't in this
            //       version due to backwards compatibility requirements.
            return new Tokenizer(input);
        }

        protected virtual Analyzer NewAnalyzer()
        {
            // TODO: This method should really be abstract, but it isn't in this
            //       version due to backwards compatibility requirements.
            return new Analyzer();
        }

        public Tokenizer Tokenizer => _tokenizer;

        public Analyzer Analyzer => _analyzer;

        public Tokenizer GetTokenizer()
        {
            return Tokenizer;
        }

        public Analyzer GetAnalyzer()
        {
            return Analyzer;
        }

        internal void SetInitialized(bool initialized)
        {
            _initialized = initialized;
        }

        public virtual void AddPattern(ProductionPattern pattern)
        {
            if (pattern.Count <= 0)
            {
                throw new ParserCreationException(
                    ParserCreationException.ErrorType.INVALID_PRODUCTION,
                    pattern.Name,
                    "no production alternatives are present (must have at " +
                    "least one)");
            }
            if (_patternIds.ContainsKey(pattern.Id))
            {
                throw new ParserCreationException(
                    ParserCreationException.ErrorType.INVALID_PRODUCTION,
                    pattern.Name,
                    "another pattern with the same id (" + pattern.Id +
                    ") has already been added");
            }
            _patterns.Add(pattern);
            _patternIds.Add(pattern.Id, pattern);
            SetInitialized(false);
        }

        public virtual void Prepare()
        {
            if (_patterns.Count <= 0)
            {
                throw new ParserCreationException(
                    ParserCreationException.ErrorType.INVALID_PARSER,
                    "no production patterns have been added");
            }
            for (int i = 0; i < _patterns.Count; i++)
            {
                CheckPattern((ProductionPattern)_patterns[i]);
            }
            SetInitialized(true);
        }

        private void CheckPattern(ProductionPattern pattern)
        {
            for (int i = 0; i < pattern.Count; i++)
            {
                CheckAlternative(pattern.Name, pattern[i]);
            }
        }

        private void CheckAlternative(string name,
                                      ProductionPatternAlternative alt)
        {

            for (int i = 0; i < alt.Count; i++)
            {
                CheckElement(name, alt[i]);
            }
        }

        
        private void CheckElement(string name,
                                  ProductionPatternElement elem)
        {

            if (elem.IsProduction() && GetPattern(elem.Id) == null)
            {
                throw new ParserCreationException(
                    ParserCreationException.ErrorType.INVALID_PRODUCTION,
                    name,
                    "an undefined production pattern id (" + elem.Id +
                    ") is referenced");
            }
        }

        public void Reset(TextReader input)
        {
            this._tokenizer.Reset(input);
            this._analyzer.Reset();
        }

        public void Reset(TextReader input, Analyzer analyzer)
        {
            this._tokenizer.Reset(input);
            this._analyzer = analyzer;
        }

        public Node Parse()
        {
            Node root = null;

            // Initialize parser
            if (!_initialized)
            {
                Prepare();
            }
            this._tokens.Clear();
            this._errorLog = new ParserLogException();
            this._errorRecovery = -1;

            // Parse input
            try
            {
                root = ParseStart();
            }
            catch (ParseException e)
            {
                AddError(e, true);
            }

            // Check for errors
            if (_errorLog.Count > 0)
            {
                throw _errorLog;
            }

            return root;
        }

        protected abstract Node ParseStart();

        protected virtual Production NewProduction(ProductionPattern pattern)
        {
            return _analyzer.NewProduction(pattern);
        }

        internal void AddError(ParseException e, bool recovery)
        {
            if (_errorRecovery <= 0)
            {
                _errorLog.AddError(e);
            }
            if (recovery)
            {
                _errorRecovery = 3;
            }
        }

        internal ProductionPattern GetPattern(int id)
        {
            return (ProductionPattern)_patternIds[id];
        }

        internal ProductionPattern GetStartPattern()
        {
            if (_patterns.Count <= 0)
            {
                return null;
            }
            else
            {
                return (ProductionPattern)_patterns[0];
            }
        }

        internal ICollection GetPatterns()
        {
            return _patterns;
        }

        internal void EnterNode(Node node)
        {
            if (!node.IsHidden() && _errorRecovery < 0)
            {
                try
                {
                    _analyzer.Enter(node);
                }
                catch (ParseException e)
                {
                    AddError(e, false);
                }
            }
        }

        internal Node ExitNode(Node node)
        {
            if (!node.IsHidden() && _errorRecovery < 0)
            {
                try
                {
                    return _analyzer.Exit(node);
                }
                catch (ParseException e)
                {
                    AddError(e, false);
                }
            }
            return node;
        }

        internal void AddNode(Production node, Node child)
        {
            if (_errorRecovery >= 0)
            {
                // Do nothing
            }
            else if (node.IsHidden())
            {
                node.AddChild(child);
            }
            else if (child != null && child.IsHidden())
            {
                for (int i = 0; i < child.Count; i++)
                {
                    AddNode(node, child[i]);
                }
            }
            else
            {
                try
                {
                    _analyzer.Child(node, child);
                }
                catch (ParseException e)
                {
                    AddError(e, false);
                }
            }
        }

        internal Token NextToken()
        {
            Token token = PeekToken(0);

            if (token != null)
            {
                _tokens.RemoveAt(0);
                return token;
            }
            else
            {
                throw new ParseException(
                    ParseException.ErrorType.UNEXPECTED_EOF,
                    null,
                    _tokenizer.GetCurrentLine(),
                    _tokenizer.GetCurrentColumn());
            }
        }

        internal Token NextToken(int id)
        {
            Token token = NextToken();

            if (token.Id == id)
            {
                if (_errorRecovery > 0)
                {
                    _errorRecovery--;
                }
                return token;
            }
            else
            {
                var list = new ArrayList(1) {_tokenizer.GetPatternDescription(id)};
                throw new ParseException(
                    ParseException.ErrorType.UNEXPECTED_TOKEN,
                    token.ToShortString(),
                    list,
                    token.StartLine,
                    token.StartColumn);
            }
        }

        internal Token PeekToken(int steps)
        {
            while (steps >= _tokens.Count)
            {
                try
                {
                    var token = _tokenizer.Next();
                    if (token == null)
                    {
                        return null;
                    }
                    else
                    {
                        _tokens.Add(token);
                    }
                }
                catch (ParseException e)
                {
                    AddError(e, true);
                }
            }
            return (Token)_tokens[steps];
        }

        public override string ToString()
        {
            StringBuilder buffer = new StringBuilder();

            for (int i = 0; i < _patterns.Count; i++)
            {
                buffer.Append(ToString((ProductionPattern)_patterns[i]));
                buffer.Append("\n");
            }
            return buffer.ToString();
        }

        private string ToString(ProductionPattern prod)
        {
            StringBuilder buffer = new StringBuilder();
            StringBuilder indent = new StringBuilder();
            int i;

            buffer.Append(prod.Name);
            buffer.Append(" (");
            buffer.Append(prod.Id);
            buffer.Append(") ");
            for (i = 0; i < buffer.Length; i++)
            {
                indent.Append(" ");
            }
            buffer.Append("= ");
            indent.Append("| ");
            for (i = 0; i < prod.Count; i++)
            {
                if (i > 0)
                {
                    buffer.Append(indent);
                }
                buffer.Append(ToString(prod[i]));
                buffer.Append("\n");
            }
            for (i = 0; i < prod.Count; i++)
            {
                var set = prod[i].LookAhead;
                if (set.GetMaxLength() > 1)
                {
                    buffer.Append("Using ");
                    buffer.Append(set.GetMaxLength());
                    buffer.Append(" token look-ahead for alternative ");
                    buffer.Append(i + 1);
                    buffer.Append(": ");
                    buffer.Append(set.ToString(_tokenizer));
                    buffer.Append("\n");
                }
            }
            return buffer.ToString();
        }

        private string ToString(ProductionPatternAlternative alt)
        {
            StringBuilder buffer = new StringBuilder();

            for (int i = 0; i < alt.Count; i++)
            {
                if (i > 0)
                {
                    buffer.Append(" ");
                }
                buffer.Append(ToString(alt[i]));
            }
            return buffer.ToString();
        }

        private string ToString(ProductionPatternElement elem)
        {
            StringBuilder buffer = new StringBuilder();
            int min = elem.MinCount;
            int max = elem.MaxCount;

            if (min == 0 && max == 1)
            {
                buffer.Append("[");
            }
            if (elem.IsToken())
            {
                buffer.Append(GetTokenDescription(elem.Id));
            }
            else
            {
                buffer.Append(GetPattern(elem.Id).Name);
            }
            if (min == 0 && max == 1)
            {
                buffer.Append("]");
            }
            else if (min == 0 && max == Int32.MaxValue)
            {
                buffer.Append("*");
            }
            else if (min == 1 && max == Int32.MaxValue)
            {
                buffer.Append("+");
            }
            else if (min != 1 || max != 1)
            {
                buffer.Append("{");
                buffer.Append(min);
                buffer.Append(",");
                buffer.Append(max);
                buffer.Append("}");
            }
            return buffer.ToString();
        }

        internal string GetTokenDescription(int token)
        {
            if (_tokenizer == null)
            {
                return "";
            }
            else
            {
                return _tokenizer.GetPatternDescription(token);
            }
        }
    }
}
