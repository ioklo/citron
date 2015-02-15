using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace RuleParser
{
    class IDExp : Exp
    {
        public String String { get; private set; }

        public IDExp(string str)
        {
            String = str;
        }
    }
}
