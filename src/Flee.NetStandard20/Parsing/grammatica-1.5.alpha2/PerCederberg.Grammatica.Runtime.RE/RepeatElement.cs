using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Flee.Parsing.grammatica_1._5.alpha2.PerCederberg.Grammatica.Runtime.RE
{

    /**
     * A regular expression element repeater. The element repeats the
     * matches from a specified element, attempting to reach the
     * maximum repetition count.
     */
    internal class RepeatElement : Element
    {
        public enum RepeatType
        {
            GREEDY = 1,
            RELUCTANT = 2,
            POSSESSIVE = 3
        }
        private readonly Element _elem;
        private readonly int _min;
        private readonly int _max;
        private readonly RepeatType _type;
        private int _matchStart;
        private BitArray _matches;

        public RepeatElement(Element elem,
                             int min,
                             int max,
                             RepeatType type)
        {

            this._elem = elem;
            this._min = min;
            if (max <= 0)
            {
                this._max = Int32.MaxValue;
            }
            else
            {
                this._max = max;
            }
            this._type = type;
            this._matchStart = -1;
            this._matches = null;
        }

        public override object Clone()
        {
            return new RepeatElement((Element)_elem.Clone(),
                                     _min,
                                     _max,
                                     _type);
        }

        public override int Match(Matcher m,
                                  ReaderBuffer buffer,
                                  int start,
                                  int skip)
        {
            if (skip == 0)
            {
                _matchStart = -1;
                _matches = null;
            }
            switch (_type)
            {
                case RepeatType.GREEDY:
                    return MatchGreedy(m, buffer, start, skip);
                case RepeatType.RELUCTANT:
                    return MatchReluctant(m, buffer, start, skip);
                case RepeatType.POSSESSIVE:
                    if (skip == 0)
                    {
                        return MatchPossessive(m, buffer, start, 0);
                    }
                    break;
            }
            return -1;
        }

        private int MatchGreedy(Matcher m,
                                ReaderBuffer buffer,
                                int start,
                                int skip)
        {
            // Check for simple case
            if (skip == 0)
            {
                return MatchPossessive(m, buffer, start, 0);
            }

            // Find all matches
            if (_matchStart != start)
            {
                _matchStart = start;
                _matches = new BitArray(10);
                FindMatches(m, buffer, start, 0, 0, 0);
            }

            // Find first non-skipped match
            for (int i = _matches.Count - 1; i >= 0; i--)
            {
                if (_matches[i])
                {
                    if (skip == 0)
                    {
                        return i;
                    }
                    skip--;
                }
            }
            return -1;
        }

        private int MatchReluctant(Matcher m,
                                   ReaderBuffer buffer,
                                   int start,
                                   int skip)
        {
            if (_matchStart != start)
            {
                _matchStart = start;
                _matches = new BitArray(10);
                FindMatches(m, buffer, start, 0, 0, 0);
            }

            // Find first non-skipped match
            for (int i = 0; i < _matches.Count; i++)
            {
                if (_matches[i])
                {
                    if (skip == 0)
                    {
                        return i;
                    }
                    skip--;
                }
            }
            return -1;
        }

        private int MatchPossessive(Matcher m,
                                    ReaderBuffer buffer,
                                    int start,
                                    int count)
        {
            int length = 0;
            int subLength = 1;

            // Match as many elements as possible
            while (subLength > 0 && count < _max)
            {
                subLength = _elem.Match(m, buffer, start + length, 0);
                if (subLength >= 0)
                {
                    count++;
                    length += subLength;
                }
            }

            // Return result
            if (_min <= count && count <= _max)
            {
                return length;
            }
            else
            {
                return -1;
            }
        }

        private void FindMatches(Matcher m,
                                 ReaderBuffer buffer,
                                 int start,
                                 int length,
                                 int count,
                                 int attempt)
        {
            int subLength;

            // Check match ending here
            if (count > _max)
            {
                return;
            }
            if (_min <= count && attempt == 0)
            {
                if (_matches.Length <= length)
                {
                    _matches.Length = length + 10;
                }
                _matches[length] = true;
            }

            // Check element match
            subLength = _elem.Match(m, buffer, start, attempt);
            if (subLength < 0)
            {
                return;
            }
            else if (subLength == 0)
            {
                if (_min == count + 1)
                {
                    if (_matches.Length <= length)
                    {
                        _matches.Length = length + 10;
                    }
                    _matches[length] = true;
                }
                return;
            }

            // Find alternative and subsequent matches
            FindMatches(m, buffer, start, length, count, attempt + 1);
            FindMatches(m,
                        buffer,
                        start + subLength,
                        length + subLength,
                        count + 1,
                        0);
        }

        public override void PrintTo(TextWriter output, string indent)
        {
            output.Write(indent + "Repeat (" + _min + "," + _max + ")");
            if (_type == RepeatType.RELUCTANT)
            {
                output.Write("?");
            }
            else if (_type == RepeatType.POSSESSIVE)
            {
                output.Write("+");
            }
            output.WriteLine();
            _elem.PrintTo(output, indent + "  ");
        }
    }
}
