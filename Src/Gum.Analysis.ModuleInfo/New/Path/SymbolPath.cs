using Gum.Collections;
using M = Gum.CompileTime;

namespace Gum.Analysis
{
    public record SymbolPath(SymbolPath? Outer, M.Name Name, ImmutableArray<SymbolId> TypeArgs = default, ImmutableArray<FuncParamId> ParamIds = default);

    public static class SymbolPathExtensions
    {
        public static SymbolPath Child(this SymbolPath? outer, M.Name name, ImmutableArray<SymbolId> typeArgs = default, ImmutableArray<FuncParamId> paramIds = default)
        {
            return new SymbolPath(outer, name, typeArgs, paramIds);
        }
    }
}