using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gum.Core.IL
{
    public class StringValue : IValue
    {
        public string Value { get; private set; }

        public StringValue(string s)
        {
            this.Value = s;
        }

        public void CopyFrom(IValue value)
        {
            Value = ((StringValue)value).Value;
        }
    }
}
