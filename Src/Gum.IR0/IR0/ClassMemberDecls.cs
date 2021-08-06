using Gum.Collections;

namespace Gum.IR0
{
    public abstract record ClassMemberDecl : Decl;

    public record ClassMemberVarDecl(AccessModifier AccessModifier, Path Type, ImmutableArray<string> Names) : ClassMemberDecl
    {
        public override void EnsurePure() { }
    }
    
    public record ClassMemberFuncDecl(ImmutableArray<Decl> Decls, Name Name, bool IsThisCall, ImmutableArray<string> TypeParams, ImmutableArray<Param> Parameters, Stmt Body) : ClassMemberDecl
    {
        public override void EnsurePure() { }
    }

    public record ClassMemberSeqFuncDecl(ImmutableArray<Decl> Decls, Name Name, bool IsThisCall, Path YieldType, ImmutableArray<string> TypeParams, ImmutableArray<Param> Parameters, Stmt Body) : ClassMemberDecl
    {
        public override void EnsurePure() { }
    }

    // public S(int a, int b) { }
    public record ClassConstructorDecl(
        AccessModifier AccessModifier,
        ImmutableArray<Decl> Decls,
        ImmutableArray<Param> Parameters,
        Stmt Body
    ) : ClassMemberDecl
    {
        public override void EnsurePure() { }
    }


}
