using Gum.Collections;
using Pretune;

namespace Gum.IR0
{
    [AutoConstructor, ImplementIEquatable]
    public partial class LambdaDecl : IDecl
    {
        public DeclId DeclId { get; }

        public Type? CapturedThisType { get; }
        public ImmutableArray<TypeAndName> CaptureInfo { get; }
        
        public ImmutableArray<ParamInfo> ParamInfos { get; }
        public Stmt Body { get; }
    }
}