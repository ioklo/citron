using Gum.Collections;
using Gum.Infra;
using Pretune;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gum.IR0
{
    [AutoConstructor, ImplementIEquatable]
    public partial struct ParamInfo : IPure
    {
        public int? VariadicParamIndex { get; }
        public ImmutableArray<TypeAndName> Parameters { get; }

        public void EnsurePure()
        {
            Misc.EnsurePure(Parameters);
        }
    }
}
