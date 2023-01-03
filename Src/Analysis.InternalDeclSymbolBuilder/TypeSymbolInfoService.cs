using Citron.Symbol;
using Pretune;
using S = Citron.Syntax;

namespace Citron.Analysis
{
    [AutoConstructor]
    public partial class TypeSymbolInfoService
    {
        SymbolLoader loader;

        public IType? GetSymbol(S.TypeExp typeExp)
        {
            var typeExpInfo = typeExp.GetTypeExpInfo();
            var typeId = typeExpInfo.GetTypeId();
            return loader.Load(symbolId) as ITypeSymbol;
        }
    }
}