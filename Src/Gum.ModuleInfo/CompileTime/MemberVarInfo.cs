using Pretune;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Gum.CompileTime
{
    [AutoConstructor, ImplementIEquatable]
    public partial class MemberVarInfo : ItemInfo
    {
        public AccessModifier AccessModifier { get; }
        public bool IsStatic { get; }
        public Type Type { get; }
        public override Name Name { get; }
    }
}
