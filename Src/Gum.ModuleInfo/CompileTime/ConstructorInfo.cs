using Gum.Collections;
using Pretune;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gum.CompileTime
{
    [AutoConstructor, ImplementIEquatable]
    public partial class ConstructorInfo : ItemInfo, ICallableInfo
    {
        public AccessModifier AccessModifier { get; }
        public override Name Name => Name.Constructor;
        public ImmutableArray<Param> Parameters { get; }
        public bool IsTrivial { get; } // 멤버에 맞춰 자동생성된 생성자인가
    }
}
