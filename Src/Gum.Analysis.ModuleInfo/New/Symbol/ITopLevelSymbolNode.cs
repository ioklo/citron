using M = Gum.CompileTime;

namespace Gum.Analysis
{
    // module, namespace
    public interface ITopLevelSymbolNode : ISymbolNode
    {
        new ITopLevelSymbolNode Apply(TypeEnv typeEnv);
        (M.Name Module, M.NamespacePath? NamespacePath) GetRootPath();
    }
}