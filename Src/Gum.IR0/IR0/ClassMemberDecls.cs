using Gum.Collections;

namespace Gum.IR0
{
    public abstract record ClassMemberDecl : Decl;

    public record ClassMemberVarDecl(AccessModifier AccessModifier, Path Type, ImmutableArray<string> Names) : ClassMemberDecl
    {
        public override void EnsurePure() { }
    }    
}
