using Gum.Collections;

namespace Gum.IR0
{
    public record StructMemberVarDecl(AccessModifier AccessModifier, Path Type, ImmutableArray<string> Names);

    // public S(int a, int b) { }
    public record StructConstructorDecl(
        AccessModifier AccessModifier,
        ImmutableArray<CallableMemberDecl> CallableMemberDecls,
        ImmutableArray<Param> Parameters,
        Stmt Body
    );
}
