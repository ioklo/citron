using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gum.App.Compiler
{
    public class FieldInfo
    {
        public TypeInfo TypeInfo { get; private set; }
        public string Name { get; private set; }
        public AccessModifier Accessor { get; private set; }
        public int Index { get; private set; }

        public FieldInfo(AccessModifier a, TypeInfo typeInfo, string name, int i)
        {
            TypeInfo = typeInfo;
            Name = name;
            Accessor = a;
            Index = i;
        }
    }
}
