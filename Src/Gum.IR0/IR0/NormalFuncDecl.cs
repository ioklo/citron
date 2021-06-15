using Pretune;
using Gum.Collections;
using Gum.Infra;

namespace Gum.IR0
{
    [AutoConstructor, ImplementIEquatable]
    public partial class NormalFuncDecl : Decl
    {
        public ImmutableArray<Decl> Decls { get; }

        public Name Name { get; }
        public bool IsThisCall { get; }
        public ImmutableArray<string> TypeParams { get; }
        public ImmutableArray<Param> Parameters { get; }
        public Stmt Body { get; }

        public override void EnsurePure()
        {
            Misc.EnsurePure(Name);            
            Misc.EnsurePure(TypeParams);
            Misc.EnsurePure(Parameters);
            Misc.EnsurePure(Body);
        }
    }
}
