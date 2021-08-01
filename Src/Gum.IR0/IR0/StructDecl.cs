using System.Collections.Generic;
using Gum.Collections;
using System.Text;
using Gum.Infra;
using Pretune;

namespace Gum.IR0
{    
    public partial record StructDecl(
        AccessModifier AccessModifier,
        string Name,
        ImmutableArray<string> TypeParams,
        ImmutableArray<Path> BaseTypes,
        ImmutableArray<StructDecl.MemberDecl> MemberDecls
    ) : Decl
    {
        
        public override void EnsurePure()
        {
            Misc.EnsurePure(TypeParams);
            Misc.EnsurePure(BaseTypes);
        }
    }

    public partial record StructDecl
    {
        public abstract record MemberDecl : Decl
        {
            public record Var(AccessModifier AccessModifier, Path Type, ImmutableArray<string> Names) : MemberDecl
            {
                public override void EnsurePure() { }
            }

            public record Func(ImmutableArray<Decl> Decls, Name Name, bool IsThisCall, ImmutableArray<string> TypeParams, ImmutableArray<Param> Parameters, Stmt Body) : MemberDecl
            {
                public override void EnsurePure() { }
            }

            public record SeqFunc(ImmutableArray<Decl> Decls, Name Name, bool IsThisCall, Path YieldType, ImmutableArray<string> TypeParams, ImmutableArray<Param> Parameters, Stmt Body) : MemberDecl
            {
                public override void EnsurePure() { }
            }

            // public S(int a, int b) { }
            public record Constructor(
                AccessModifier AccessModifier,
                ImmutableArray<Decl> Decls,
                ImmutableArray<Param> Parameters,
                Stmt Body
            ) : MemberDecl
            {
                public override void EnsurePure() { }
            }
        }
    }
}
