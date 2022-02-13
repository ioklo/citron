using Gum.Collections;

namespace Gum.CompileTime
{
    public record SymbolPath(SymbolPath? Outer, Name Name, ImmutableArray<SymbolId> TypeArgs = default, ImmutableArray<FuncParamId> ParamIds = default);

    public static class SymbolPathExtensions
    {
        public static SymbolPath Child(this SymbolPath? outer, Name name, ImmutableArray<SymbolId> typeArgs = default, ImmutableArray<FuncParamId> paramIds = default)
        {
            return new SymbolPath(outer, name, typeArgs, paramIds);
        }
    }
}