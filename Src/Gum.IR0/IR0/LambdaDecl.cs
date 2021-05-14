using Gum.Collections;
using Pretune;

namespace Gum.IR0
{
    [AutoConstructor, ImplementIEquatable]
    public partial class LambdaDecl : Decl
    {
        public Name.Anonymous Name { get; }
        public CapturedStatement CapturedStatement { get; }
        public ParamInfo ParamInfo { get; }
    }
}