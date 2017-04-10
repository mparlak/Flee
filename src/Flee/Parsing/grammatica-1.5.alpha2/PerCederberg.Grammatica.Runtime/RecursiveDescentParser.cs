using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Flee.Parsing.grammatica_1._5.alpha2.PerCederberg.Grammatica.Runtime;

namespace Flee.Parsing.grammatica_1._5.alpha2.PerCederberg.Grammatica.Runtime
{
    /**
     * A recursive descent parser. This parser handles LL(n) grammars,
     * selecting the appropriate pattern to parse based on the next few
     * tokens. The parser is more efficient the fewer look-ahead tokens
     * that is has to consider.
     */
    internal class RecursiveDescentParser : Parser
    {
        public RecursiveDescentParser(TextReader input) : base(input)
        {
        }

        public RecursiveDescentParser(TextReader input, Analyzer analyzer)
            : base(input, analyzer)
        {
        }

        public RecursiveDescentParser(Tokenizer tokenizer)
            : base(tokenizer)
        {
        }

        public RecursiveDescentParser(Tokenizer tokenizer,
                                      Analyzer analyzer)
            : base(tokenizer, analyzer)
        {
        }

        public override void AddPattern(ProductionPattern pattern)
        {

            // Check for empty matches
            if (pattern.IsMatchingEmpty())
            {
                throw new ParserCreationException(
                    ParserCreationException.ErrorType.INVALID_PRODUCTION,
                    pattern.Name,
                    "zero elements can be matched (minimum is one)");
            }

            // Check for left-recusive patterns
            if (pattern.IsLeftRecursive())
            {
                throw new ParserCreationException(
                    ParserCreationException.ErrorType.INVALID_PRODUCTION,
                    pattern.Name,
                    "left recursive patterns are not allowed");
            }

            // Add pattern
            base.AddPattern(pattern);
        }

        public override void Prepare()
        {
            // Performs production pattern checks
            base.Prepare();
            SetInitialized(false);

            // Calculate production look-ahead sets
            var e = GetPatterns().GetEnumerator();
            while (e.MoveNext())
            {
                CalculateLookAhead((ProductionPattern)e.Current);
            }

            // Set initialized flag
            SetInitialized(true);
        }

        protected override Node ParseStart()
        {
            var node = ParsePattern(GetStartPattern());
            var token = PeekToken(0);
            if (token != null)
            {
                var list = new ArrayList(1) { "<EOF>" };
                throw new ParseException(
                    ParseException.ErrorType.UNEXPECTED_TOKEN,
                    token.ToShortString(),
                    list,
                    token.StartLine,
                    token.StartColumn);
            }
            return node;
        }

        private Node ParsePattern(ProductionPattern pattern)
        {
            var defaultAlt = pattern.DefaultAlternative;
            for (int i = 0; i < pattern.Count; i++)
            {
                var alt = pattern[i];
                if (defaultAlt != alt && IsNext(alt))
                {
                    return ParseAlternative(alt);
                }
            }
            if (defaultAlt == null || !IsNext(defaultAlt))
            {
                ThrowParseException(FindUnion(pattern));
            }
            return ParseAlternative(defaultAlt);
        }

        private Node ParseAlternative(ProductionPatternAlternative alt)
        {
            var node = NewProduction(alt.Pattern);
            EnterNode(node);
            for (int i = 0; i < alt.Count; i++)
            {
                try
                {
                    ParseElement(node, alt[i]);
                }
                catch (ParseException e)
                {
                    AddError(e, true);
                    NextToken();
                    i--;
                }
            }
            return ExitNode(node);
        }

        private void ParseElement(Production node,
                                  ProductionPatternElement elem)
        {
            for (int i = 0; i < elem.MaxCount; i++)
            {
                if (i < elem.MinCount || IsNext(elem))
                {
                    Node child;
                    if (elem.IsToken())
                    {
                        child = NextToken(elem.Id);
                        EnterNode(child);
                        AddNode(node, ExitNode(child));
                    }
                    else
                    {
                        child = ParsePattern(GetPattern(elem.Id));
                        AddNode(node, child);
                    }
                }
                else
                {
                    break;
                }
            }
        }

        private bool IsNext(ProductionPattern pattern)
        {
            LookAheadSet set = pattern.LookAhead;

            if (set == null)
            {
                return false;
            }
            else
            {
                return set.IsNext(this);
            }
        }

        private bool IsNext(ProductionPatternAlternative alt)
        {
            LookAheadSet set = alt.LookAhead;

            if (set == null)
            {
                return false;
            }
            else
            {
                return set.IsNext(this);
            }
        }

        private bool IsNext(ProductionPatternElement elem)
        {
            LookAheadSet set = elem.LookAhead;

            if (set != null)
            {
                return set.IsNext(this);
            }
            else if (elem.IsToken())
            {
                return elem.IsMatch(PeekToken(0));
            }
            else
            {
                return IsNext(GetPattern(elem.Id));
            }
        }

        private void CalculateLookAhead(ProductionPattern pattern)
        {
            ProductionPatternAlternative alt;
            LookAheadSet previous = new LookAheadSet(0);
            int length = 1;
            int i;
            CallStack stack = new CallStack();

            // Calculate simple look-ahead
            stack.Push(pattern.Name, 1);
            var result = new LookAheadSet(1);
            var alternatives = new LookAheadSet[pattern.Count];
            for (i = 0; i < pattern.Count; i++)
            {
                alt = pattern[i];
                alternatives[i] = FindLookAhead(alt, 1, 0, stack, null);
                alt.LookAhead = alternatives[i];
                result.AddAll(alternatives[i]);
            }
            if (pattern.LookAhead == null)
            {
                pattern.LookAhead = result;
            }
            var conflicts = FindConflicts(pattern, 1);

            // Resolve conflicts
            while (conflicts.Size() > 0)
            {
                length++;
                stack.Clear();
                stack.Push(pattern.Name, length);
                conflicts.AddAll(previous);
                for (i = 0; i < pattern.Count; i++)
                {
                    alt = pattern[i];
                    if (alternatives[i].Intersects(conflicts))
                    {
                        alternatives[i] = FindLookAhead(alt,
                                                        length,
                                                        0,
                                                        stack,
                                                        conflicts);
                        alt.LookAhead = alternatives[i];
                    }
                    if (alternatives[i].Intersects(conflicts))
                    {
                        if (pattern.DefaultAlternative == null)
                        {
                            pattern.DefaultAlternative = alt;
                        }
                        else if (pattern.DefaultAlternative != alt)
                        {
                            result = alternatives[i].CreateIntersection(conflicts);
                            ThrowAmbiguityException(pattern.Name,
                                                    null,
                                                    result);
                        }
                    }
                }
                previous = conflicts;
                conflicts = FindConflicts(pattern, length);
            }

            // Resolve conflicts inside rules
            for (i = 0; i < pattern.Count; i++)
            {
                CalculateLookAhead(pattern[i], 0);
            }
        }

        private void CalculateLookAhead(ProductionPatternAlternative alt,
                                        int pos)
        {
            LookAheadSet previous = new LookAheadSet(0);
            int length = 1;

            // Check trivial cases
            if (pos >= alt.Count)
            {
                return;
            }

            // Check for non-optional element
            var pattern = alt.Pattern;
            var elem = alt[pos];
            if (elem.MinCount == elem.MaxCount)
            {
                CalculateLookAhead(alt, pos + 1);
                return;
            }

            // Calculate simple look-aheads
            var first = FindLookAhead(elem, 1, new CallStack(), null);
            var follow = FindLookAhead(alt, 1, pos + 1, new CallStack(), null);

            // Resolve conflicts
            var location = "at position " + (pos + 1);
            var conflicts = FindConflicts(pattern.Name,
                location,
                first,
                follow);
            while (conflicts.Size() > 0)
            {
                length++;
                conflicts.AddAll(previous);
                first = FindLookAhead(elem,
                                      length,
                                      new CallStack(),
                                      conflicts);
                follow = FindLookAhead(alt,
                                       length,
                                       pos + 1,
                                       new CallStack(),
                                       conflicts);
                first = first.CreateCombination(follow);
                elem.LookAhead = first;
                if (first.Intersects(conflicts))
                {
                    first = first.CreateIntersection(conflicts);
                    ThrowAmbiguityException(pattern.Name, location, first);
                }
                previous = conflicts;
                conflicts = FindConflicts(pattern.Name,
                                          location,
                                          first,
                                          follow);
            }

            // Check remaining elements
            CalculateLookAhead(alt, pos + 1);
        }

        private LookAheadSet FindLookAhead(ProductionPattern pattern,
                                           int length,
                                           CallStack stack,
                                           LookAheadSet filter)
        {
            // Check for infinite loop
            if (stack.Contains(pattern.Name, length))
            {
                throw new ParserCreationException(
                    ParserCreationException.ErrorType.INFINITE_LOOP,
                    pattern.Name,
                    (String)null);
            }

            // Find pattern look-ahead
            stack.Push(pattern.Name, length);
            var result = new LookAheadSet(length);
            for (int i = 0; i < pattern.Count; i++)
            {
                var temp = FindLookAhead(pattern[i],
                    length,
                    0,
                    stack,
                    filter);
                result.AddAll(temp);
            }
            stack.Pop();

            return result;
        }

        private LookAheadSet FindLookAhead(ProductionPatternAlternative alt,
                                           int length,
                                           int pos,
                                           CallStack stack,
                                           LookAheadSet filter)
        {
            LookAheadSet follow;
            // Check trivial cases
            if (length <= 0 || pos >= alt.Count)
            {
                return new LookAheadSet(0);
            }

            // Find look-ahead for this element
            var first = FindLookAhead(alt[pos], length, stack, filter);
            if (alt[pos].MinCount == 0)
            {
                first.AddEmpty();
            }

            // Find remaining look-ahead
            if (filter == null)
            {
                length -= first.GetMinLength();
                if (length > 0)
                {
                    follow = FindLookAhead(alt, length, pos + 1, stack, null);
                    first = first.CreateCombination(follow);
                }
            }
            else if (filter.IsOverlap(first))
            {
                var overlaps = first.CreateOverlaps(filter);
                length -= overlaps.GetMinLength();
                filter = filter.CreateFilter(overlaps);
                follow = FindLookAhead(alt, length, pos + 1, stack, filter);
                first.RemoveAll(overlaps);
                first.AddAll(overlaps.CreateCombination(follow));
            }

            return first;
        }

        private LookAheadSet FindLookAhead(ProductionPatternElement elem,
                                           int length,
                                           CallStack stack,
                                           LookAheadSet filter)
        {
            // Find initial element look-ahead
            var first = FindLookAhead(elem, length, 0, stack, filter);
            var result = new LookAheadSet(length);
            result.AddAll(first);
            if (filter == null || !filter.IsOverlap(result))
            {
                return result;
            }

            // Handle element repetitions
            if (elem.MaxCount == Int32.MaxValue)
            {
                first = first.CreateRepetitive();
            }
            var max = elem.MaxCount;
            if (length < max)
            {
                max = length;
            }
            for (int i = 1; i < max; i++)
            {
                first = first.CreateOverlaps(filter);
                if (first.Size() <= 0 || first.GetMinLength() >= length)
                {
                    break;
                }
                var follow = FindLookAhead(elem,
                    length,
                    0,
                    stack,
                    filter.CreateFilter(first));
                first = first.CreateCombination(follow);
                result.AddAll(first);
            }

            return result;
        }

        private LookAheadSet FindLookAhead(ProductionPatternElement elem,
                                           int length,
                                           int dummy,
                                           CallStack stack,
                                           LookAheadSet filter)
        {
            LookAheadSet result;

            if (elem.IsToken())
            {
                result = new LookAheadSet(length);
                result.Add(elem.Id);
            }
            else
            {
                var pattern = GetPattern(elem.Id);
                result = FindLookAhead(pattern, length, stack, filter);
                if (stack.Contains(pattern.Name))
                {
                    result = result.CreateRepetitive();
                }
            }

            return result;
        }

        private LookAheadSet FindConflicts(ProductionPattern pattern,
                                           int maxLength)
        {

            LookAheadSet result = new LookAheadSet(maxLength);
            for (int i = 0; i < pattern.Count; i++)
            {
                var set1 = pattern[i].LookAhead;
                for (int j = 0; j < i; j++)
                {
                    var set2 = pattern[j].LookAhead;
                    result.AddAll(set1.CreateIntersection(set2));
                }
            }
            if (result.IsRepetitive())
            {
                ThrowAmbiguityException(pattern.Name, null, result);
            }
            return result;
        }

        private LookAheadSet FindConflicts(string pattern,
                                           string location,
                                           LookAheadSet set1,
                                           LookAheadSet set2)
        {
            var result = set1.CreateIntersection(set2);
            if (result.IsRepetitive())
            {
                ThrowAmbiguityException(pattern, location, result);
            }
            return result;
        }

        private LookAheadSet FindUnion(ProductionPattern pattern)
        {
            LookAheadSet result;
            int length = 0;
            int i;

            for (i = 0; i < pattern.Count; i++)
            {
                result = pattern[i].LookAhead;
                if (result.GetMaxLength() > length)
                {
                    length = result.GetMaxLength();
                }
            }
            result = new LookAheadSet(length);
            for (i = 0; i < pattern.Count; i++)
            {
                result.AddAll(pattern[i].LookAhead);
            }

            return result;
        }

        private void ThrowParseException(LookAheadSet set)
        {
            ArrayList list = new ArrayList();

            // Read tokens until mismatch
            while (set.IsNext(this, 1))
            {
                set = set.CreateNextSet(NextToken().Id);
            }

            // Find next token descriptions
            var initials = set.GetInitialTokens();
            for (int i = 0; i < initials.Length; i++)
            {
                list.Add(GetTokenDescription(initials[i]));
            }

            // Create exception
            var token = NextToken();
            throw new ParseException(ParseException.ErrorType.UNEXPECTED_TOKEN,
                                     token.ToShortString(),
                                     list,
                                     token.StartLine,
                                     token.StartColumn);
        }

        private void ThrowAmbiguityException(string pattern,
                                             string location,
                                             LookAheadSet set)
        {

            ArrayList list = new ArrayList();

            // Find next token descriptions
            var initials = set.GetInitialTokens();
            for (int i = 0; i < initials.Length; i++)
            {
                list.Add(GetTokenDescription(initials[i]));
            }

            // Create exception
            throw new ParserCreationException(
                ParserCreationException.ErrorType.INHERENT_AMBIGUITY,
                pattern,
                location,
                list);
        }


        private class CallStack
        {
            private readonly ArrayList _nameStack = new ArrayList();
            private readonly ArrayList _valueStack = new ArrayList();
            public bool Contains(string name)
            {
                return _nameStack.Contains(name);
            }

            public bool Contains(string name, int value)
            {
                for (int i = 0; i < _nameStack.Count; i++)
                {
                    if (_nameStack[i].Equals(name)
                     && _valueStack[i].Equals(value))
                    {

                        return true;
                    }
                }
                return false;
            }

            public void Clear()
            {
                _nameStack.Clear();
                _valueStack.Clear();
            }

            public void Push(string name, int value)
            {
                _nameStack.Add(name);
                _valueStack.Add(value);
            }

            public void Pop()
            {
                if (_nameStack.Count > 0)
                {
                    _nameStack.RemoveAt(_nameStack.Count - 1);
                    _valueStack.RemoveAt(_valueStack.Count - 1);
                }
            }
        }
    }
}
