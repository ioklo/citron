using Citron.Infra;

namespace Citron.Symbol
{
    public interface IFuncSymbol : ISymbolNode, ICyclicEqualityComparableClass<IFuncSymbol>
    {
        new IFuncSymbol Apply(TypeEnv typeEnv);
        new IFuncDeclSymbol GetDeclSymbolNode();

        ITypeSymbol? GetOuterType();

        int GetParameterCount();
        FuncParameter GetParameter(int index);
    }
}