using Citron.Infra;

namespace Citron.Symbol
{
    public interface IFuncDeclSymbol : IDeclSymbolNode, ICyclicEqualityComparableClass<IFuncDeclSymbol>
    {
        int GetParameterCount();
        FuncParameter GetParameter(int index);
    }
}