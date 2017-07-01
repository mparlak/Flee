using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using Flee.Parsing.grammatica_1._5.alpha2.PerCederberg.Grammatica.Runtime;

namespace Flee.Parsing.grammatica_1._5.alpha2.PerCederberg.Grammatica.Runtime
{

    /**
    * A production node. This class represents a grammar production
    * (i.e. a list of child nodes) in a parse tree. The productions
    * are created by a parser, that adds children a according to a
    * set of production patterns (i.e. grammar rules).
    */
    internal class Production : Node
    {
        private readonly ProductionPattern _pattern;
        private readonly ArrayList _children;

        public Production(ProductionPattern pattern)
        {
            this._pattern = pattern;
            this._children = new ArrayList();
        }

        public override int Id => _pattern.Id;

        public override string Name => _pattern.Name;

        public override int Count => _children.Count;

        public override Node this[int index]
        {
            get
            {
                if (index < 0 || index >= _children.Count)
                {
                    return null;
                }
                else
                {
                    return (Node)_children[index];
                }
            }
        }

        public void AddChild(Node child)
        {
            if (child != null)
            {
                child.SetParent(this);
                _children.Add(child);
            }
        }

        public ProductionPattern Pattern => _pattern;

        public ProductionPattern GetPattern()
        {
            return Pattern;
        }

        internal override bool IsHidden()
        {
            return _pattern.Synthetic;
        }

        public override string ToString()
        {
            return _pattern.Name + '(' + _pattern.Id + ')';
        }
    }
}
