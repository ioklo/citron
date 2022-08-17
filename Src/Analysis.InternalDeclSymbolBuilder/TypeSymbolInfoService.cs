using Citron.Symbol;
using Pretune;
using S = Citron.Syntax;

namespace Citron.Analysis
{
    [AutoConstructor]
    public partial class TypeSymbolInfoService
    {
        SymbolLoader loader;

        public ITypeSymbol? GetSymbol(S.TypeExp typeExp)
        {
            var symbolId = typeExp.Info.GetSymbolId();
            return loader.Load(symbolId) as ITypeSymbol;
        }
    }
}