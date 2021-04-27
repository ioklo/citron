using Gum.Collections;
using Pretune;

namespace Gum.IR0
{
    [AutoConstructor, ImplementIEquatable]
    public partial class LambdaDecl : IDecl
    {
        public DeclId DeclId { get; }
        public CapturedStatement CapturedStatement { get; }
        public ImmutableArray<ParamInfo> ParamInfos { get; }
    }
}