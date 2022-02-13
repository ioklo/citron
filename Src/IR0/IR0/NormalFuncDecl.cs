using Pretune;
using Citron.Collections;
using Citron.Infra;

namespace Citron.IR0
{
    public record NormalFuncDecl(ImmutableArray<CallableMemberDecl> CallableMemberDecls, Name Name, bool IsThisCall, ImmutableArray<string> TypeParams, ImmutableArray<Param> Parameters, Stmt Body) : FuncDecl;
}
