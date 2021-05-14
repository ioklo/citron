using Pretune;
using Gum.Collections;

namespace Gum.IR0
{
    [AutoConstructor, ImplementIEquatable]
    public partial class NormalFuncDecl : Decl
    {
        public ImmutableArray<Decl> Decls { get; }

        public Name Name { get; }
        public bool IsThisCall { get; }
        public ImmutableArray<string> TypeParams { get; }
        public ParamInfo ParamInfo { get; }
        public Stmt Body { get; }
    }
}
