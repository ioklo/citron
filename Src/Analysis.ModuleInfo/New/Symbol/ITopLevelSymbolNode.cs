using M = Citron.CompileTime;

namespace Citron.Analysis
{
    // module, namespace
    public interface ITopLevelSymbolNode : ISymbolNode
    {
        new ITopLevelSymbolNode Apply(TypeEnv typeEnv);
        (M.Name Module, M.NamespacePath? NamespacePath) GetRootPath();

        SymbolQueryResult QueryMember(M.Name memberName, int typeParamCount);
    }
}