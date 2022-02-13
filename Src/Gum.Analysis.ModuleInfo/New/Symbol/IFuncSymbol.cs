namespace Citron.Analysis
{
    public interface IFuncSymbol : ISymbolNode
    {
        new IFuncSymbol Apply(TypeEnv typeEnv);
        new IFuncDeclSymbol GetDeclSymbolNode();

        ITypeSymbol? GetOuterType();

        int GetParameterCount();
        FuncParameter GetParameter(int index);
    }
}