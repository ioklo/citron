using Citron.Collections;
using Citron.Module;
using Pretune;

namespace Citron.Symbol
{
    [ImplementIEquatable]
    public record SymbolPath
    {
        public SymbolPath? Outer { get; set; }
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