using Citron.IR1;
using System;
using System.Collections.Generic;
using Citron.Collections;
using System.Text;

namespace Citron.IR1.Runtime
{
    public partial class Evaluator
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
