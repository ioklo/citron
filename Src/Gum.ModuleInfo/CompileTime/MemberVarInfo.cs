using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gum.CompileTime
{
    public class MemberVarInfo : ItemInfo
    {
        public bool IsStatic { get; }
        public Type Type { get; }
        public override Name Name { get; }

        public MemberVarInfo(bool bStatic, Type type, Name name)
        {
            IsStatic = bStatic;
            Type = type;
            Name = name;
        }
    }
}
