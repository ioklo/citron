using Gum.Core.IL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gum.Core.Runtime
{
    public class FuncValue : IValue
    {
        public IFunction Value { get; private set; }

        public FuncValue(IFunction value)
        {
            Value = value;
        }

        public void CopyFrom(IValue value)
        {
            Value = ((FuncValue)value).Value;
        }
    }
}
