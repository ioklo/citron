using Gum.Collections;
using Gum.Infra;
using Pretune;

namespace Gum.IR0
{
    [AutoConstructor, ImplementIEquatable]
    public partial class LambdaDecl : Decl
    {
        public Name.Anonymous Name { get; }
        public CapturedStatement CapturedStatement { get; }
        public ParamInfo ParamInfo { get; }

        public override void EnsurePure()
        {
            Misc.EnsurePure(Name);
            Misc.EnsurePure(CapturedStatement);
            Misc.EnsurePure(ParamInfo);
        }
    }
}