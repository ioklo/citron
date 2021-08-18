using Gum.Infra;
using Pretune;

namespace Gum.IR0
{
    public record CapturedStatementDecl(Name.Anonymous Name, CapturedStatement CapturedStatement) : CallableMemberDecl;
}