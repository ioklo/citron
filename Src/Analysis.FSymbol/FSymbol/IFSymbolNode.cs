namespace Citron.Analysis
{
    public interface IFSymbolNode
    {
        FSymbolOuter GetOuter();
        IFSymbolNode Apply(TypeEnv typeEnv);
        TypeEnv GetTypeEnv();
    }
}