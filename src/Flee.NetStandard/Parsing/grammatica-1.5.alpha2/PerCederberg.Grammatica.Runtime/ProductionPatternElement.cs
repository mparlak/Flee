using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Flee.Parsing.grammatica_1._5.alpha2.PerCederberg.Grammatica.Runtime
{
    /**
      * A production pattern element. This class represents a reference to
      * either a token or a production. Each element also contains minimum
      * and maximum occurence counters, controlling the number of
      * repetitions allowed. A production pattern element is always
      * contained within a production pattern rule.
      */
    internal class ProductionPatternElement
    {
        private readonly bool _token;
        private readonly int _id;
        private readonly int _min;
        private readonly int _max;
        private LookAheadSet _lookAhead;

        public ProductionPatternElement(bool isToken,
                                        int id,
                                        int min,
                                        int max)
        {

            this._token = isToken;
            this._id = id;
            if (min < 0)
            {
                min = 0;
            }
            this._min = min;
            if (max <= 0)
            {
                max = Int32.MaxValue;
            }
            else if (max < min)
            {
                max = min;
            }
            this._max = max;
            this._lookAhead = null;
        }

        public int Id => _id;

        public int GetId()
        {
            return Id;
        }

        public int MinCount => _min;

        public int GetMinCount()
        {
            return MinCount;
        }

        public int MaxCount => _max;

        public int GetMaxCount()
        {
            return MaxCount;
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

        public bool IsToken()
        {
            return _token;
        }

        public bool IsProduction()
        {
            return !_token;
        }

        public bool IsMatch(Token token)
        {
            return IsToken() && token != null && token.Id == _id;
        }

        public override bool Equals(object obj)
        {
            if (obj is ProductionPatternElement)
            {
                var elem = (ProductionPatternElement)obj;
                return this._token == elem._token
                    && this._id == elem._id
                    && this._min == elem._min
                    && this._max == elem._max;
            }
            else
            {
                return false;
            }
        }

        public override int GetHashCode()
        {
            return this._id * 37;
        }

        public override string ToString()
        {
            StringBuilder buffer = new StringBuilder();

            buffer.Append(_id);
            if (_token)
            {
                buffer.Append("(Token)");
            }
            else
            {
                buffer.Append("(Production)");
            }
            if (_min != 1 || _max != 1)
            {
                buffer.Append("{");
                buffer.Append(_min);
                buffer.Append(",");
                buffer.Append(_max);
                buffer.Append("}");
            }
            return buffer.ToString();
        }
    }
}
