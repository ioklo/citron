using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gum.Core.IL
{
    public class BoolValue : IValue
    {
        public bool Value { get; private set; }

        public BoolValue(bool value)
        {
            this.Value = value;
        }
 
        public void CopyFrom(IValue value)
        {
            Value = ((BoolValue)value).Value;
        }
    }
}
