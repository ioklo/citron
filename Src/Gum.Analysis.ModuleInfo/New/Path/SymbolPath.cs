using Gum.Collections;
using M = Gum.CompileTime;

namespace Gum.Analysis
{
    public record SymbolPath(SymbolPath? Outer, M.Name Name, ImmutableArray<SymbolId> TypeArgs = default, M.ParamTypes ParamTypes = default);

    public static class SymbolPathExtensions
    {
        public static SymbolPath Child(this SymbolPath? outer, M.Name name, ImmutableArray<SymbolId> typeArgs = default, M.ParamTypes paramTypes = default)
        {
            return new SymbolPath(outer, name, typeArgs, paramTypes);
        }
    }
}