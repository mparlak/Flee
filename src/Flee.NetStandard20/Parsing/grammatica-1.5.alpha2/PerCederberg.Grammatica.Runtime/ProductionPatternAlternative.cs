using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Flee.Parsing.grammatica_1._5.alpha2.PerCederberg.Grammatica.Runtime
{

    /**
     * A production pattern alternative. This class represents a list of
     * production pattern elements. In order to provide productions that
     * cannot be represented with the element occurance counters, multiple
     * alternatives must be created and added to the same production
     * pattern. A production pattern alternative is always contained
     * within a production pattern.
     */
    internal class ProductionPatternAlternative
    {
        private ProductionPattern _pattern;
        private readonly ArrayList _elements = new ArrayList();
        private LookAheadSet _lookAhead = null;

        public ProductionPatternAlternative()
        {
        }

        public ProductionPattern Pattern => _pattern;

        public ProductionPattern GetPattern()
        {
            return Pattern;
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

        public int Count => _elements.Count;

        public int GetElementCount()
        {
            return Count;
        }

        public ProductionPatternElement this[int index] => (ProductionPatternElement)_elements[index];

        public ProductionPatternElement GetElement(int pos)
        {
            return this[pos];
        }

        public bool IsLeftRecursive()
        {
            for (int i = 0; i < _elements.Count; i++)
            {
                var elem = (ProductionPatternElement)_elements[i];
                if (elem.Id == _pattern.Id)
                {
                    return true;
                }
                else if (elem.MinCount > 0)
                {
                    break;
                }
            }
            return false;
        }

        public bool IsRightRecursive()
        {
            for (int i = _elements.Count - 1; i >= 0; i--)
            {
                var elem = (ProductionPatternElement)_elements[i];
                if (elem.Id == _pattern.Id)
                {
                    return true;
                }
                else if (elem.MinCount > 0)
                {
                    break;
                }
            }
            return false;
        }

        public bool IsMatchingEmpty()
        {
            return GetMinElementCount() == 0;
        }

        internal void SetPattern(ProductionPattern pattern)
        {
            this._pattern = pattern;
        }

        public int GetMinElementCount()
        {
            int min = 0;

            for (int i = 0; i < _elements.Count; i++)
            {
                var elem = (ProductionPatternElement)_elements[i];
                min += elem.MinCount;
            }
            return min;
        }

        public int GetMaxElementCount()
        {
            int max = 0;

            for (int i = 0; i < _elements.Count; i++)
            {
                var elem = (ProductionPatternElement)_elements[i];
                if (elem.MaxCount >= Int32.MaxValue)
                {
                    return Int32.MaxValue;
                }
                else
                {
                    max += elem.MaxCount;
                }
            }
            return max;
        }

        public void AddToken(int id, int min, int max)
        {
            AddElement(new ProductionPatternElement(true, id, min, max));
        }

        public void AddProduction(int id, int min, int max)
        {
            AddElement(new ProductionPatternElement(false, id, min, max));
        }

        public void AddElement(ProductionPatternElement elem)
        {
            _elements.Add(elem);
        }

        public void AddElement(ProductionPatternElement elem,
                               int min,
                               int max)
        {

            if (elem.IsToken())
            {
                AddToken(elem.Id, min, max);
            }
            else
            {
                AddProduction(elem.Id, min, max);
            }
        }

        public override bool Equals(object obj)
        {
            if (obj is ProductionPatternAlternative)
            {
                return Equals((ProductionPatternAlternative)obj);
            }
            else
            {
                return false;
            }
        }

        public bool Equals(ProductionPatternAlternative alt)
        {
            if (_elements.Count != alt._elements.Count)
            {
                return false;
            }
            for (int i = 0; i < _elements.Count; i++)
            {
                if (!_elements[i].Equals(alt._elements[i]))
                {
                    return false;
                }
            }
            return true;
        }

        public override int GetHashCode()
        {
            return _elements.Count.GetHashCode();
        }

        public override string ToString()
        {
            StringBuilder buffer = new StringBuilder();

            for (int i = 0; i < _elements.Count; i++)
            {
                if (i > 0)
                {
                    buffer.Append(" ");
                }
                buffer.Append(_elements[i]);
            }
            return buffer.ToString();
        }
    }
}
