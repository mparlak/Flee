

using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;


namespace Flee.Parsing.grammatica_1._5.alpha2.PerCederberg.Grammatica.Runtime.RE
{
    /**
      * A regular expression alternative element. This element matches
      * the longest alternative element.
      */
    internal class AlternativeElement : Element
    {
        private readonly Element _elem1;
        private readonly Element _elem2;

        public AlternativeElement(Element first, Element second)
        {
            _elem1 = first;
            _elem2 = second;
        }

        public override object Clone()
        {
            return new AlternativeElement(_elem1, _elem2);
        }

        public override int Match(Matcher m,
                                  ReaderBuffer buffer,
                                  int start,
                                  int skip)
        {
            int length = 0;
            int length1 = -1;
            int length2 = -1;
            int skip1 = 0;
            int skip2 = 0;

            while (length >= 0 && skip1 + skip2 <= skip)
            {
                length1 = _elem1.Match(m, buffer, start, skip1);
                length2 = _elem2.Match(m, buffer, start, skip2);
                if (length1 >= length2)
                {
                    length = length1;
                    skip1++;
                }
                else
                {
                    length = length2;
                    skip2++;
                }
            }
            return length;
        }

        public override void PrintTo(TextWriter output, string indent)
        {
            output.WriteLine(indent + "Alternative 1");
            _elem1.PrintTo(output, indent + "  ");
            output.WriteLine(indent + "Alternative 2");
            _elem2.PrintTo(output, indent + "  ");
        }
    }
}
