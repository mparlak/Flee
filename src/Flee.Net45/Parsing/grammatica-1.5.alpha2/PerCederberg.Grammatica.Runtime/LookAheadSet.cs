using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Flee.Parsing.grammatica_1._5.alpha2.PerCederberg.Grammatica.Runtime
{
    /*
      * A token look-ahead set. This class contains a set of token id
      * sequences. All sequences in the set are limited in length, so
      * that no single sequence is longer than a maximum value. This
      * class also filters out duplicates. Each token sequence also
      * contains a repeat flag, allowing the look-ahead set to contain
      * information about possible infinite repetitions of certain
      * sequences. That information is important when conflicts arise
      * between two look-ahead sets, as such a conflict cannot be
      * resolved if the conflicting sequences can be repeated (would
      * cause infinite loop).
      */
    internal class LookAheadSet
    {
        private readonly ArrayList _elements = new ArrayList();
        private readonly int _maxLength;

        public LookAheadSet(int maxLength)
        {
            this._maxLength = maxLength;
        }

        public LookAheadSet(int maxLength, LookAheadSet set)
            : this(maxLength)
        {

            AddAll(set);
        }

        public int Size()
        {
            return _elements.Count;
        }

        public int GetMinLength()
        {
            int min = -1;

            for (int i = 0; i < _elements.Count; i++)
            {
                var seq = (Sequence)_elements[i];
                if (min < 0 || seq.Length() < min)
                {
                    min = seq.Length();
                }
            }
            return (min < 0) ? 0 : min;
        }

        public int GetMaxLength()
        {
            int max = 0;
            for (int i = 0; i < _elements.Count; i++)
            {
                var seq = (Sequence)_elements[i];
                if (seq.Length() > max)
                {
                    max = seq.Length();
                }
            }
            return max;
        }

        public int[] GetInitialTokens()
        {
            ArrayList list = new ArrayList();
            int i;
            for (i = 0; i < _elements.Count; i++)
            {
                var token = ((Sequence)_elements[i]).GetToken(0);
                if (token != null && !list.Contains(token))
                {
                    list.Add(token);
                }
            }
            var result = new int[list.Count];
            for (i = 0; i < list.Count; i++)
            {
                result[i] = (int)list[i];
            }
            return result;
        }

        public bool IsRepetitive()
        {
            for (int i = 0; i < _elements.Count; i++)
            {
                var seq = (Sequence)_elements[i];
                if (seq.IsRepetitive())
                {
                    return true;
                }
            }
            return false;
        }

        public bool IsNext(Parser parser)
        {
            for (int i = 0; i < _elements.Count; i++)
            {
                var seq = (Sequence)_elements[i];
                if (seq.IsNext(parser))
                {
                    return true;
                }
            }
            return false;
        }

        public bool IsNext(Parser parser, int length)
        {
            for (int i = 0; i < _elements.Count; i++)
            {
                var seq = (Sequence)_elements[i];
                if (seq.IsNext(parser, length))
                {
                    return true;
                }
            }
            return false;
        }

        public bool IsOverlap(LookAheadSet set)
        {
            for (int i = 0; i < _elements.Count; i++)
            {
                if (set.IsOverlap((Sequence)_elements[i]))
                {
                    return true;
                }
            }
            return false;
        }

        private bool IsOverlap(Sequence seq)
        {
            for (int i = 0; i < _elements.Count; i++)
            {
                var elem = (Sequence)_elements[i];
                if (seq.StartsWith(elem) || elem.StartsWith(seq))
                {
                    return true;
                }
            }
            return false;
        }

        private bool Contains(Sequence elem)
        {
            return FindSequence(elem) != null;
        }

        public bool Intersects(LookAheadSet set)
        {
            for (int i = 0; i < _elements.Count; i++)
            {
                if (set.Contains((Sequence)_elements[i]))
                {
                    return true;
                }
            }
            return false;
        }

        private Sequence FindSequence(Sequence elem)
        {
            for (int i = 0; i < _elements.Count; i++)
            {
                if (_elements[i].Equals(elem))
                {
                    return (Sequence)_elements[i];
                }
            }
            return null;
        }

        private void Add(Sequence seq)
        {
            if (seq.Length() > _maxLength)
            {
                seq = new Sequence(_maxLength, seq);
            }
            if (!Contains(seq))
            {
                _elements.Add(seq);
            }
        }

        public void Add(int token)
        {
            Add(new Sequence(false, token));
        }

        public void AddAll(LookAheadSet set)
        {
            for (int i = 0; i < set._elements.Count; i++)
            {
                Add((Sequence)set._elements[i]);
            }
        }

        public void AddEmpty()
        {
            Add(new Sequence());
        }

        private void Remove(Sequence seq)
        {
            _elements.Remove(seq);
        }

        public void RemoveAll(LookAheadSet set)
        {
            for (int i = 0; i < set._elements.Count; i++)
            {
                Remove((Sequence)set._elements[i]);
            }
        }

        public LookAheadSet CreateNextSet(int token)
        {
            LookAheadSet result = new LookAheadSet(_maxLength - 1);
            for (int i = 0; i < _elements.Count; i++)
            {
                var seq = (Sequence)_elements[i];
                var value = seq.GetToken(0);
                if (value != null && token == (int)value)
                {
                    result.Add(seq.Subsequence(1));
                }
            }
            return result;
        }

        public LookAheadSet CreateIntersection(LookAheadSet set)
        {
            LookAheadSet result = new LookAheadSet(_maxLength);
            for (int i = 0; i < _elements.Count; i++)
            {
                var seq1 = (Sequence)_elements[i];
                var seq2 = set.FindSequence(seq1);
                if (seq2 != null && seq1.IsRepetitive())
                {
                    result.Add(seq2);
                }
                else if (seq2 != null)
                {
                    result.Add(seq1);
                }
            }
            return result;
        }

        public LookAheadSet CreateCombination(LookAheadSet set)
        {
            LookAheadSet result = new LookAheadSet(_maxLength);

            // Handle special cases
            if (this.Size() <= 0)
            {
                return set;
            }
            else if (set.Size() <= 0)
            {
                return this;
            }

            // Create combinations
            for (int i = 0; i < _elements.Count; i++)
            {
                var first = (Sequence)_elements[i];
                if (first.Length() >= _maxLength)
                {
                    result.Add(first);
                }
                else if (first.Length() <= 0)
                {
                    result.AddAll(set);
                }
                else
                {
                    for (int j = 0; j < set._elements.Count; j++)
                    {
                        var second = (Sequence)set._elements[j];
                        result.Add(first.Concat(_maxLength, second));
                    }
                }
            }
            return result;
        }

        public LookAheadSet CreateOverlaps(LookAheadSet set)
        {
            LookAheadSet result = new LookAheadSet(_maxLength);

            for (int i = 0; i < _elements.Count; i++)
            {
                var seq = (Sequence)_elements[i];
                if (set.IsOverlap(seq))
                {
                    result.Add(seq);
                }
            }
            return result;
        }

        public LookAheadSet CreateFilter(LookAheadSet set)
        {
            LookAheadSet result = new LookAheadSet(_maxLength);

            // Handle special cases
            if (this.Size() <= 0 || set.Size() <= 0)
            {
                return this;
            }

            // Create combinations
            for (int i = 0; i < _elements.Count; i++)
            {
                var first = (Sequence)_elements[i];
                for (int j = 0; j < set._elements.Count; j++)
                {
                    var second = (Sequence)set._elements[j];
                    if (first.StartsWith(second))
                    {
                        result.Add(first.Subsequence(second.Length()));
                    }
                }
            }
            return result;
        }

        public LookAheadSet CreateRepetitive()
        {
            LookAheadSet result = new LookAheadSet(_maxLength);

            for (int i = 0; i < _elements.Count; i++)
            {
                var seq = (Sequence)_elements[i];
                if (seq.IsRepetitive())
                {
                    result.Add(seq);
                }
                else
                {
                    result.Add(new Sequence(true, seq));
                }
            }
            return result;
        }

        public override string ToString()
        {
            return ToString(null);
        }

        public string ToString(Tokenizer tokenizer)
        {
            StringBuilder buffer = new StringBuilder();

            buffer.Append("{");
            for (int i = 0; i < _elements.Count; i++)
            {
                var seq = (Sequence)_elements[i];
                buffer.Append("\n  ");
                buffer.Append(seq.ToString(tokenizer));
            }
            buffer.Append("\n}");
            return buffer.ToString();
        }

        private class Sequence
        {
            private bool _repeat;
            private readonly ArrayList _tokens;

            public Sequence()
            {
                this._repeat = false;
                this._tokens = new ArrayList(0);
            }

            public Sequence(bool repeat, int token)
            {
                _repeat = false;
                _tokens = new ArrayList(1);
                _tokens.Add(token);
            }

            public Sequence(int length, Sequence seq)
            {
                this._repeat = seq._repeat;
                this._tokens = new ArrayList(length);
                if (seq.Length() < length)
                {
                    length = seq.Length();
                }
                for (int i = 0; i < length; i++)
                {
                    _tokens.Add(seq._tokens[i]);
                }
            }

            public Sequence(bool repeat, Sequence seq)
            {
                this._repeat = repeat;
                this._tokens = seq._tokens;
            }

            public int Length()
            {
                return _tokens.Count;
            }

            public object GetToken(int pos)
            {
                if (pos >= 0 && pos < _tokens.Count)
                {
                    return _tokens[pos];
                }
                else
                {
                    return null;
                }
            }

            public override bool Equals(object obj)
            {
                if (obj is Sequence)
                {
                    return Equals((Sequence)obj);
                }
                else
                {
                    return false;
                }
            }

            public bool Equals(Sequence seq)
            {
                if (_tokens.Count != seq._tokens.Count)
                {
                    return false;
                }
                for (int i = 0; i < _tokens.Count; i++)
                {
                    if (!_tokens[i].Equals(seq._tokens[i]))
                    {
                        return false;
                    }
                }
                return true;
            }

            public override int GetHashCode()
            {
                return _tokens.Count.GetHashCode();
            }

            public bool StartsWith(Sequence seq)
            {
                if (Length() < seq.Length())
                {
                    return false;
                }
                for (int i = 0; i < seq._tokens.Count; i++)
                {
                    if (!_tokens[i].Equals(seq._tokens[i]))
                    {
                        return false;
                    }
                }
                return true;
            }

            public bool IsRepetitive()
            {
                return _repeat;
            }

            public bool IsNext(Parser parser)
            {
                for (int i = 0; i < _tokens.Count; i++)
                {
                    var id = (int)_tokens[i];
                    var token = parser.PeekToken(i);
                    if (token == null || token.Id != id)
                    {
                        return false;
                    }
                }
                return true;
            }

            public bool IsNext(Parser parser, int length)
            {
                if (length > _tokens.Count)
                {
                    length = _tokens.Count;
                }
                for (int i = 0; i < length; i++)
                {
                    var id = (int)_tokens[i];
                    var token = parser.PeekToken(i);
                    if (token == null || token.Id != id)
                    {
                        return false;
                    }
                }
                return true;
            }

            public override string ToString()
            {
                return ToString(null);
            }

            public string ToString(Tokenizer tokenizer)
            {
                StringBuilder buffer = new StringBuilder();

                if (tokenizer == null)
                {
                    buffer.Append(_tokens.ToString());
                }
                else
                {
                    buffer.Append("[");
                    for (int i = 0; i < _tokens.Count; i++)
                    {
                        var id = (int)_tokens[i];
                        var str = tokenizer.GetPatternDescription(id);
                        if (i > 0)
                        {
                            buffer.Append(" ");
                        }
                        buffer.Append(str);
                    }
                    buffer.Append("]");
                }
                if (_repeat)
                {
                    buffer.Append(" *");
                }
                return buffer.ToString();
            }

            public Sequence Concat(int length, Sequence seq)
            {
                Sequence res = new Sequence(length, this);

                if (seq._repeat)
                {
                    res._repeat = true;
                }
                length -= this.Length();
                if (length > seq.Length())
                {
                    res._tokens.AddRange(seq._tokens);
                }
                else
                {
                    for (int i = 0; i < length; i++)
                    {
                        res._tokens.Add(seq._tokens[i]);
                    }
                }
                return res;
            }

            public Sequence Subsequence(int start)
            {
                Sequence res = new Sequence(Length(), this);

                while (start > 0 && res._tokens.Count > 0)
                {
                    res._tokens.RemoveAt(0);
                    start--;
                }
                return res;
            }
        }
    }
}
