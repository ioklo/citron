using Gum.Collections;
using Pretune;

namespace Gum.IR0
{
    [AutoConstructor, ImplementIEquatable]
    public partial class LambdaDecl : Decl
    {
        public LambdaId Id { get; } // local id
        public CapturedStatement CapturedStatement { get; }
        public ImmutableArray<ParamInfo> ParamInfos { get; }
    }
}