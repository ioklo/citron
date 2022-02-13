using Citron.Infra;
using Pretune;

namespace Citron.IR0
{
    public record CapturedStatementDecl(Name.Anonymous Name, CapturedStatement CapturedStatement) : CallableMemberDecl;
}