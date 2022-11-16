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
            var typeExpInfo = typeExp.GetTypeExpInfo();
            var symbolId = typeExpInfo.GetSymbolId();
            return loader.Load(symbolId) as ITypeSymbol;
        }
    }
}