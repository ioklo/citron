using System;
using Citron.Collections;
using Citron.Symbol;

namespace Citron.IR2;

// static한 program data
public struct Program
{
    ModuleDeclSymbol declSymbol;
    ImmutableDictionary<DeclSymbolId, FuncInfo> funcInfos;

    public FuncInfo GetFuncInfo(DeclSymbolId declSymbolId)
    {
        return funcInfos[declSymbolId];
    }
}

