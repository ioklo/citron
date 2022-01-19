namespace Gum.Analysis
{
    public interface IFuncSymbol : ISymbolNode
    {
        new IFuncSymbol Apply(TypeEnv typeEnv);

        ITypeSymbol? GetOuterType();

        int GetParameterCount();
        FuncParameter GetParameter(int index);
    }
}