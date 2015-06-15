using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gum.Core.AbstractSyntax
{
    public class TypeIdentifier
    {
        public string Value { get; private set; }

        public TypeIdentifier(string value)
        {
            Value = value;
        }
    }
}
