﻿using Citron.Collections;
using Citron.Module;
using Pretune;

namespace Citron.Symbol
{    
    public record SymbolPath
    {
        public SymbolPath? Outer { get; set; } // 오픈한 이유는
        public Name Name { get; }
        public ImmutableArray<SymbolId> TypeArgs { get; }
        public ImmutableArray<FuncParamId> ParamIds { get; } 

        public SymbolPath(SymbolPath? outer, Name name, ImmutableArray<SymbolId> typeArgs = default, ImmutableArray<FuncParamId> paramIds = default)
        {
            Outer = outer;
            Name = name;
            TypeArgs = typeArgs;
            ParamIds = paramIds;
        }
    }

    public static class SymbolPathExtensions
    {
        public static SymbolPath Child(this SymbolPath? outer, Name name, ImmutableArray<SymbolId> typeArgs = default, ImmutableArray<FuncParamId> paramIds = default)
        {
            return new SymbolPath(outer, name, typeArgs, paramIds);
        }
    }
}