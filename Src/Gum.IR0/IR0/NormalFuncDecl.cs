using Pretune;
using Gum.Collections;

namespace Gum.IR0
{
    [AutoConstructor, ImplementIEquatable]
    public partial class NormalFuncDecl : Decl
    {
        public Name Name { get; }
        public bool IsThisCall { get; }
        public ImmutableArray<string> TypeParams { get; }
        public ImmutableArray<ParamInfo> ParamInfos { get; }
        public Stmt Body { get; }        
    }
}
