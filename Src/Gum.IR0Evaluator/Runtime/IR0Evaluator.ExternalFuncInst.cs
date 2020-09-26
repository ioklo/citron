using Gum.IR0;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;

namespace Gum.Runtime
{
    public partial class IR0Evaluator
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
