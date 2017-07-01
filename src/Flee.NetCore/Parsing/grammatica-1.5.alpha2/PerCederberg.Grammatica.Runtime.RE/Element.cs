using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Diagnostics;

namespace Flee.Parsing.grammatica_1._5.alpha2.PerCederberg.Grammatica.Runtime.RE
{
    /**
     * A regular expression element. This is the common base class for
     * all regular expression elements, i.e. the parts of the regular
     * expression.
     */
    internal abstract class Element : ICloneable
    {
        public abstract object Clone();

        public abstract int Match(Matcher m,
                                  ReaderBuffer buffer,
                                  int start,
                                  int skip);
       
        public abstract void PrintTo(TextWriter output, string indent);
    }
}
