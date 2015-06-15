using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gum.Core.IL
{
    public class FieldInfo
    {
        public IType Type { get; private set; }
        public int Index { get; private set; }

        public FieldInfo(IType type, int index)
        {
            Type = type;
            Index = index;
        }
    }
}
