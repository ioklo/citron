using Gum.CompileTime;
using Pretune;
using System;
using System.Collections.Generic;
using Gum.Collections;
using System.Diagnostics;
using System.Linq;
using System.Xml.XPath;
using Gum.Infra;

namespace Gum.IR0
{
    public interface IStmtVisitor
    {
        void VisitCommandStmt(CommandStmt commandStmt);                     
        void VisitGlobalVarDeclStmt(GlobalVarDeclStmt globalVarDeclStmt);
        void VisitLocalVarDeclStmt(LocalVarDeclStmt localVarDeclStmt);
        void VisitIfStmt(IfStmt ifStmt);
        void VisitIfTestClassStmt(IfTestClassStmt ifTestClassStmt);
        void VisitIfTestEnumElemStmt(IfTestEnumElemStmt ifTestEnumElemStmt);
        void VisitForStmt(ForStmt forStmt);
        void VisitContinueStmt(ContinueStmt continueStmt);
        void VisitBreakStmt(BreakStmt breakStmt);
        void VisitReturnStmt(ReturnStmt returnStmt);
        void VisitBlockStmt(BlockStmt blockStmt);
        void VisitBlankStmt(BlankStmt blankStmt);
        void VisitExpStmt(ExpStmt expStmt);
        void VisitTaskStmt(TaskStmt taskStmt);
        void VisitAwaitStmt(AwaitStmt awaitStmt);
        void VisitAsyncStmt(AsyncStmt asyncStmt);
        void VisitForeachStmt(ForeachStmt foreachStmt);
        void VisitYieldStmt(YieldStmt yieldStmt);
        void VisitDirectiveStmt(DirectiveStmt yieldStmt);
    }

    public abstract record Stmt : IPure, INode
    {
        public void EnsurePure() { }
        public abstract void VisitRef<TVisitor>(ref TVisitor visitor) where TVisitor : struct, IStmtVisitor;
        public abstract void Visit<TVisitor>(TVisitor visitor) where TVisitor : class, IStmtVisitor;
    }

    public record CommandStmt(ImmutableArray<StringExp> Commands) : Stmt
    {
        public override void VisitRef<TVisitor>(ref TVisitor visitor)
            => visitor.VisitCommandStmt(this);

        public override void Visit<TVisitor>(TVisitor visitor)
            => visitor.VisitCommandStmt(this);
    }

    // 글로벌 변수 선언
    public record GlobalVarDeclStmt(ImmutableArray<VarDeclElement> Elems) : Stmt
    {
        public override void VisitRef<TVisitor>(ref TVisitor visitor)
            => visitor.VisitGlobalVarDeclStmt(this);

        public override void Visit<TVisitor>(TVisitor visitor)
            => visitor.VisitGlobalVarDeclStmt(this);
    }

    public record LocalVarDeclStmt(LocalVarDecl VarDecl) : Stmt
    {
        public override void VisitRef<TVisitor>(ref TVisitor visitor)
            => visitor.VisitLocalVarDeclStmt(this);

        public override void Visit<TVisitor>(TVisitor visitor)
            => visitor.VisitLocalVarDeclStmt(this);
    }

    public record IfStmt(Exp Cond, Stmt Body, Stmt? ElseBody) : Stmt
    {
        public override void VisitRef<TVisitor>(ref TVisitor visitor)
            => visitor.VisitIfStmt(this);

        public override void Visit<TVisitor>(TVisitor visitor)
            => visitor.VisitIfStmt(this);
    }

    public record IfTestClassStmt(Loc Target, Path TestType, Name VarName, Stmt Body, Stmt? ElseBody) : Stmt
    {
        public override void VisitRef<TVisitor>(ref TVisitor visitor)
            => visitor.VisitIfTestClassStmt(this);

        public override void Visit<TVisitor>(TVisitor visitor)
            => visitor.VisitIfTestClassStmt(this);
    }

    public record IfTestEnumElemStmt(Loc Target, Path.Nested EnumElem, string? VarName, Stmt Body, Stmt? ElseBody) : Stmt
    {
        public override void VisitRef<TVisitor>(ref TVisitor visitor)
            => visitor.VisitIfTestEnumElemStmt(this);

        public override void Visit<TVisitor>(TVisitor visitor)
            => visitor.VisitIfTestEnumElemStmt(this);
    }

    public record ForStmt(ForStmtInitializer? Initializer, Exp? CondExp, Exp? ContinueExp, Stmt Body) : Stmt
    {
        public override void VisitRef<TVisitor>(ref TVisitor visitor)
            => visitor.VisitForStmt(this);

        public override void Visit<TVisitor>(TVisitor visitor)
            => visitor.VisitForStmt(this);
    }

    // singleton
    public record ContinueStmt : Stmt
    {
        public static readonly ContinueStmt Instance = new ContinueStmt();
        ContinueStmt() { }

        public override void VisitRef<TVisitor>(ref TVisitor visitor)
            => visitor.VisitContinueStmt(this);

        public override void Visit<TVisitor>(TVisitor visitor)
            => visitor.VisitContinueStmt(this);
    }

    // singleton
    public record BreakStmt : Stmt
    {
        public static readonly BreakStmt Instance = new BreakStmt();
        BreakStmt() { }
        public override void VisitRef<TVisitor>(ref TVisitor visitor)
            => visitor.VisitBreakStmt(this);

        public override void Visit<TVisitor>(TVisitor visitor)
            => visitor.VisitBreakStmt(this);
    }

    public abstract record ReturnInfo
    {
        public record Ref(Loc Loc) : ReturnInfo;
        public record Expression(Exp Exp) : ReturnInfo;
        public record None : ReturnInfo
        {
            public static readonly None Instance = new None();
            None() { }
        }
    }
    
    public record ReturnStmt(ReturnInfo Info): Stmt
    {
        public override void VisitRef<TVisitor>(ref TVisitor visitor)
            => visitor.VisitReturnStmt(this);

        public override void Visit<TVisitor>(TVisitor visitor)
            => visitor.VisitReturnStmt(this);
    }

    public record BlockStmt(ImmutableArray<Stmt> Stmts) : Stmt
    {
        public override void VisitRef<TVisitor>(ref TVisitor visitor)
            => visitor.VisitBlockStmt(this);

        public override void Visit<TVisitor>(TVisitor visitor)
            => visitor.VisitBlockStmt(this);
    }

    // singleton
    public record BlankStmt : Stmt
    {
        public static readonly BlankStmt Instance = new BlankStmt();
        BlankStmt() { }
    
        public override void VisitRef<TVisitor>(ref TVisitor visitor)
            => visitor.VisitBlankStmt(this);

        public override void Visit<TVisitor>(TVisitor visitor)
            => visitor.VisitBlankStmt(this);
    }

    public record ExpStmt(Exp Exp) : Stmt
    {
        public override void VisitRef<TVisitor>(ref TVisitor visitor)
            => visitor.VisitExpStmt(this);

        public override void Visit<TVisitor>(TVisitor visitor)
            => visitor.VisitExpStmt(this);
    }

    public record TaskStmt(Path.Nested CapturedStatementDecl) : Stmt
    {
        public override void VisitRef<TVisitor>(ref TVisitor visitor)
            => visitor.VisitTaskStmt(this);

        public override void Visit<TVisitor>(TVisitor visitor)
            => visitor.VisitTaskStmt(this);
    }

    public record AwaitStmt(Stmt Body) : Stmt
    {
        public override void VisitRef<TVisitor>(ref TVisitor visitor)
            => visitor.VisitAwaitStmt(this);

        public override void Visit<TVisitor>(TVisitor visitor)
            => visitor.VisitAwaitStmt(this);
    }

    public record AsyncStmt(Path.Nested CapturedStatementDecl) : Stmt
    {
        public override void VisitRef<TVisitor>(ref TVisitor visitor)
            => visitor.VisitAsyncStmt(this);

        public override void Visit<TVisitor>(TVisitor visitor)
            => visitor.VisitAsyncStmt(this);
    }

    public record ForeachStmt(Path ElemType, string ElemName, Loc Iterator, Stmt Body) : Stmt
    {
        public override void VisitRef<TVisitor>(ref TVisitor visitor)
            => visitor.VisitForeachStmt(this);

        public override void Visit<TVisitor>(TVisitor visitor)
            => visitor.VisitForeachStmt(this);
    }

    public record YieldStmt(Exp Value) : Stmt
    {
        public override void VisitRef<TVisitor>(ref TVisitor visitor)
            => visitor.VisitYieldStmt(this);

        public override void Visit<TVisitor>(TVisitor visitor)
            => visitor.VisitYieldStmt(this);
    }

    public abstract record DirectiveStmt() : Stmt
    {
        // init은 지원하지 않기로,
        // public record Init(ImmutableArray<Loc> Locs) : DirectiveStmt;     
        public override void VisitRef<TVisitor>(ref TVisitor visitor)
            => visitor.VisitDirectiveStmt(this);

        public override void Visit<TVisitor>(TVisitor visitor)
            => visitor.VisitDirectiveStmt(this);
    }    
}
