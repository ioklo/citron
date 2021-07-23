using Gum.Infra;
using Pretune;

namespace Gum.IR0
{
    public record CapturedStatementDecl(Name.Anonymous Name, CapturedStatement CapturedStatement) : Decl
    {
        public override void EnsurePure()
        {
            Misc.EnsurePure(Name);
            Misc.EnsurePure(CapturedStatement);
        }
    }
}