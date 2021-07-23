using Pretune;
using Gum.Collections;
using Gum.Infra;

namespace Gum.IR0
{
    public record NormalFuncDecl(ImmutableArray<Decl> Decls, Name Name, bool IsThisCall, ImmutableArray<string> TypeParams, ImmutableArray<Param> Parameters, Stmt Body) : Decl
    {
        public override void EnsurePure()
        {
            Misc.EnsurePure(Name);            
            Misc.EnsurePure(TypeParams);
            Misc.EnsurePure(Parameters);
            Misc.EnsurePure(Body);
        }
    }
}
