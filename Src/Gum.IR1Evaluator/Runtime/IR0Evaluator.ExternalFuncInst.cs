using Gum.IR1;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;

namespace Gum.Runtime
{
    public partial class IR1Evaluator
    {
        struct ExternalFuncInst
        {
            public ImmutableArray<AllocInfoId> AllocInfoIds { get; }
            public ExternalFuncDelegate Delegate { get; }

            public ExternalFuncInst(IEnumerable<AllocInfoId> allocInfoIds, ExternalFuncDelegate d)
            {
                AllocInfoIds = allocInfoIds.ToImmutableArray();
                Delegate = d;
            }
        }
    }
}
