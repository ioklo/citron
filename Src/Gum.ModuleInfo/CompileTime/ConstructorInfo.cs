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
        public override Name Name => SpecialNames.Constructor;
        public ImmutableArray<Param> Parameters { get; }
    }
}
