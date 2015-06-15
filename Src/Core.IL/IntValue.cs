using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gum.Core.IL
{
    public class IntValue : IValue
    {
        public int Value { get; private set; }

        public IntValue(int i)
        {
            this.Value = i;
        }

        public void CopyFrom(IValue value)
        {
            Value = ((IntValue)value).Value;
        }
    }
}
