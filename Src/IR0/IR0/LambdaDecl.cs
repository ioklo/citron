using Citron.Collections;
using Citron.Infra;
using Pretune;

namespace Citron.IR0
{
    public record LambdaDecl(
        Name.Anonymous Name,
        CapturedStatement CapturedStatement,
        ImmutableArray<Param> Parameters
    ) : CallableMemberDecl;
}