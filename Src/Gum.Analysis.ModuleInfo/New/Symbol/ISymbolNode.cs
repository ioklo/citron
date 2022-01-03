using Gum.Collections;
using M = Gum.CompileTime;
using R = Gum.IR0;


namespace Gum.Analysis
{
    // 자식으로 가는 줄기는 없다
    public interface ISymbolNode
    {
        // 순회
        ISymbolNode? GetOuter();
        IDeclSymbolNode GetDeclSymbolNode();
        
        TypeEnv GetTypeEnv();        
        R.Path.Normal MakeRPath();
        ISymbolNode Apply(TypeEnv typeEnv);
        M.NormalTypeId MakeChildTypeId(M.Name name, ImmutableArray<M.TypeId> typeArgs);
        ImmutableArray<ITypeSymbolNode> GetTypeArgs();
    }
}