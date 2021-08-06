using Gum.Collections;

namespace Gum.IR0
{
    public abstract record StructMemberDecl : Decl;

    public record StructMemberVarDecl(AccessModifier AccessModifier, Path Type, ImmutableArray<string> Names) : StructMemberDecl
    {
        public override void EnsurePure() { }
    }

    public record StructMemberFuncDecl(ImmutableArray<Decl> Decls, Name Name, bool IsThisCall, ImmutableArray<string> TypeParams, ImmutableArray<Param> Parameters, Stmt Body) : StructMemberDecl
    {
        public override void EnsurePure() { }
    }

    public record StructMemberSeqFuncDecl(ImmutableArray<Decl> Decls, Name Name, bool IsThisCall, Path YieldType, ImmutableArray<string> TypeParams, ImmutableArray<Param> Parameters, Stmt Body) : StructMemberDecl
    {
        public override void EnsurePure() { }
    }

    // public S(int a, int b) { }
    public record StructConstructorDecl(
        AccessModifier AccessModifier,
        ImmutableArray<Decl> Decls,
        ImmutableArray<Param> Parameters,
        Stmt Body
    ) : StructMemberDecl
    {
        public override void EnsurePure() { }
    }

}
