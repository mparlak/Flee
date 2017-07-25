using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;


namespace Flee.Parsing.grammatica_1._5.alpha2.PerCederberg.Grammatica.Runtime
{

    /**
     * A production pattern. This class represents a set of production
     * alternatives that together forms a single production. A
     * production pattern is identified by an integer id and a name,
     * both provided upon creation. The pattern id is used for
     * referencing the production pattern from production pattern
     * elements.
     */
    internal class ProductionPattern
    {

        private readonly int _id;
        private readonly string _name;
        private bool _synthetic;
        private readonly ArrayList _alternatives;
        private int _defaultAlt;
        private LookAheadSet _lookAhead;

        public ProductionPattern(int id, string name)
        {
            this._id = id;
            this._name = name;
            this._synthetic = false;
            this._alternatives = new ArrayList();
            this._defaultAlt = -1;
            this._lookAhead = null;
        }
        public int Id => _id;

        public int GetId()
        {
            return Id;
        }

        public string Name => _name;

        public string GetName()
        {
            return Name;
        }

        public bool Synthetic
        {
            get
            {
                return _synthetic;
            }
            set
            {
                _synthetic = value;
            }
        }

        public bool IsSyntetic()
        {
            return Synthetic;
        }

        public void SetSyntetic(bool synthetic)
        {
            Synthetic = synthetic;
        }

        internal LookAheadSet LookAhead
        {
            get
            {
                return _lookAhead;
            }
            set
            {
                _lookAhead = value;
            }
        }

        internal ProductionPatternAlternative DefaultAlternative
        {
            get
            {
                if (_defaultAlt >= 0)
                {
                    object obj = _alternatives[_defaultAlt];
                    return (ProductionPatternAlternative)obj;
                }
                else
                {
                    return null;
                }
            }
            set
            {
                _defaultAlt = 0;
                for (int i = 0; i < _alternatives.Count; i++)
                {
                    if (_alternatives[i] == value)
                    {
                        _defaultAlt = i;
                    }
                }
            }
        }

        public int Count => _alternatives.Count;

        public int GetAlternativeCount()
        {
            return Count;
        }

        public ProductionPatternAlternative this[int index] => (ProductionPatternAlternative)_alternatives[index];

        public ProductionPatternAlternative GetAlternative(int pos)
        {
            return this[pos];
        }

        public bool IsLeftRecursive()
        {
            ProductionPatternAlternative alt;

            for (int i = 0; i < _alternatives.Count; i++)
            {
                alt = (ProductionPatternAlternative)_alternatives[i];
                if (alt.IsLeftRecursive())
                {
                    return true;
                }
            }
            return false;
        }

        public bool IsRightRecursive()
        {
            ProductionPatternAlternative alt;

            for (int i = 0; i < _alternatives.Count; i++)
            {
                alt = (ProductionPatternAlternative)_alternatives[i];
                if (alt.IsRightRecursive())
                {
                    return true;
                }
            }
            return false;
        }

        public bool IsMatchingEmpty()
        {
            ProductionPatternAlternative alt;

            for (int i = 0; i < _alternatives.Count; i++)
            {
                alt = (ProductionPatternAlternative)_alternatives[i];
                if (alt.IsMatchingEmpty())
                {
                    return true;
                }
            }
            return false;
        }

        public void AddAlternative(ProductionPatternAlternative alt)
        {
            if (_alternatives.Contains(alt))
            {
                throw new ParserCreationException(
                    ParserCreationException.ErrorType.INVALID_PRODUCTION,
                    _name,
                    "two identical alternatives exist");
            }
            alt.SetPattern(this);
            _alternatives.Add(alt);
        }

        public override string ToString()
        {
            StringBuilder buffer = new StringBuilder();
            StringBuilder indent = new StringBuilder();
            int i;

            buffer.Append(_name);
            buffer.Append("(");
            buffer.Append(_id);
            buffer.Append(") ");
            for (i = 0; i < buffer.Length; i++)
            {
                indent.Append(" ");
            }
            for (i = 0; i < _alternatives.Count; i++)
            {
                if (i == 0)
                {
                    buffer.Append("= ");
                }
                else
                {
                    buffer.Append("\n");
                    buffer.Append(indent);
                    buffer.Append("| ");
                }
                buffer.Append(_alternatives[i]);
            }
            return buffer.ToString();
        }
    }
}
