using System.Collections.Generic;
using Citron.Collections;

namespace Citron.IR1; // TODO: namespace 정책 변경..
                      // 

public class Script
{   
    public ImmutableArray<ExternalFunc> ExFuncs { get; }
    public ImmutableArray<GlobalVar> GlobalVars { get; }
    public ImmutableArray<Func> Funcs { get; }
    public FuncId EntryId { get; }
    public Script(IEnumerable<ExternalFunc> exFuncs, IEnumerable<GlobalVar> globalVars, IEnumerable<Func> funcs, FuncId entryId)
    {
        ExFuncs = exFuncs.ToImmutableArray();
        GlobalVars = globalVars.ToImmutableArray();
        Funcs = funcs.ToImmutableArray();
        EntryId = entryId;
    }
}
