using Pretune;
using S = Citron.Syntax;

namespace Citron.Analysis
{
    [AutoConstructor]
    public partial class TypeSymbolInfoService
    {
        TypeExpInfoService typeExpInfoService;
        SymbolLoader loader;

        public ITypeSymbol? GetSymbol(S.TypeExp typeExp)
        {
            var info = typeExpInfoService.GetTypeExpInfo(typeExp);
            var symbolId = info.GetSymbolId();
            return loader.Load(symbolId) as ITypeSymbol;
        }
    }
}