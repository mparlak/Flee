using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flee.Parsing.grammatica_1._5.alpha2.PerCederberg.Grammatica.Runtime
{
    /**
     * The token match status. This class contains logic to ensure that
     * only the longest match is considered. It also prefers lower token
     * pattern identifiers if two matches have the same length.
     */
    internal class TokenMatch
    {
        private int _length = 0;
        private TokenPattern _pattern = null;
       
        public void Clear()
        {
            _length = 0;
            _pattern = null;
        }

        public int Length => _length;

        public TokenPattern Pattern => _pattern;

        public void Update(int length, TokenPattern pattern)
        {
            if (this._length < length)
            {
                this._length = length;
                this._pattern = pattern;
            }
            else if (this._length == length && this._pattern.Id > pattern.Id)
            {
                this._length = length;
                this._pattern = pattern;
            }
        }
    }
}
