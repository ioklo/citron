using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gum.Core.IL
{
    public class RefValue : IValue
    {
        // Value Pointer
        public IValue Value { get; private set; }

        public RefValue(IValue value)
        {
            Value = value;
        }

        public void Set(IValue value)
        {
            Value = value;
        }

        public void CopyFrom(IValue refValue)
        {
            Value = ((RefValue)refValue).Value;
        }        
    }
}
