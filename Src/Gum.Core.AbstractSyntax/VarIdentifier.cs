using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gum.Core.AbstractSyntax
{
    public class VarIdentifier
    {
        public string Value { get; private set; }

        public VarIdentifier(string value)
        {
            Value = value;
        }
    }
}
