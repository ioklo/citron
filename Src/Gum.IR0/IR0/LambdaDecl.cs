using Gum.Collections;
using Gum.Infra;
using Pretune;

namespace Gum.IR0
{
    public record LambdaDecl(
        Name.Anonymous Name,
        CapturedStatement CapturedStatement,
        ImmutableArray<Param> Parameters
    ) : Decl
    {   
        public override void EnsurePure()
        {
            Misc.EnsurePure(Name);
            Misc.EnsurePure(CapturedStatement);
            Misc.EnsurePure(Parameters);
        }
    }
}