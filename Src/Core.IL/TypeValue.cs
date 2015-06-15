using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gum.Core.IL
{
    public class TypeValue : IValue
    {
        public IType Value { get; private set; }

        public TypeValue(IType value)
        {
            Value = value;
        }

        public void CopyFrom(IValue value)
        {
            Value = ((TypeValue)value).Value;
        }
    }
}
