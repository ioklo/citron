using Citron.Syntax;
using System;
using System.Collections.Generic;
using Citron.Collections;
using System.Diagnostics;
using System.Linq;
using System.Xml.XPath;
using Pretune;

namespace Citron.Syntax
{
    public abstract record class Stmt : ISyntaxNode
    {
        public abstract void Accept<TStmtVisitor>(ref TStmtVisitor visitor)
            where TStmtVisitor : struct, IStmtVisitor;
    }

    // if, for 등에 나오는 한줄짜리 Stmt
    public abstract record class EmbeddableStmt
    {
        public record class Single(Stmt Stmt) : EmbeddableStmt;
        public record class Multiple(ImmutableArray<Stmt> Stmts) : EmbeddableStmt;
    }

    // 명령어
    // TODO: commands의 Length가 1인 contract를 추가하자
    public record CommandStmt(ImmutableArray<StringExp> Commands) : Stmt
    {
        public override void Accept<TStmtVisitor>(ref TStmtVisitor visitor) => visitor.VisitCommand(this);
    }

    // int a = 0, b, c;
    public record class VarDeclStmt(VarDecl VarDecl) : Stmt
    {
        public override void Accept<TStmtVisitor>(ref TStmtVisitor visitor) => visitor.VisitVarDecl(this);
    }

    // if ($cond) $body else $ElseBody
    public record class IfStmt(Exp Cond, EmbeddableStmt Body, EmbeddableStmt? ElseBody) : Stmt
    {
        public override void Accept<TStmtVisitor>(ref TStmtVisitor visitor) => visitor.VisitIf(this);
    }

    public record class IfTestStmt(Exp Exp, TypeExp TestType, string? VarName, EmbeddableStmt Body, EmbeddableStmt? ElseBody) : Stmt
    {
        public override void Accept<TStmtVisitor>(ref TStmtVisitor visitor) => visitor.VisitIfTest(this);
    }

    public record class ForStmt(ForStmtInitializer? Initializer, Exp? CondExp, Exp? ContinueExp, EmbeddableStmt Body) : Stmt
    {
        public override void Accept<TStmtVisitor>(ref TStmtVisitor visitor) => visitor.VisitFor(this);
    }

    public record ContinueStmt : Stmt
    {
        public static readonly ContinueStmt Instance = new ContinueStmt();
        ContinueStmt() { }
        public override void Accept<TStmtVisitor>(ref TStmtVisitor visitor) => visitor.VisitContinue(this);
    }

    public record class BreakStmt : Stmt
    {
        public static readonly BreakStmt Instance = new BreakStmt();
        BreakStmt() { }
        public override void Accept<TStmtVisitor>(ref TStmtVisitor visitor) => visitor.VisitBreak(this);
    }

    [AutoConstructor, ImplementIEquatable]
    public partial struct ReturnValueInfo
    {
        public bool IsRef { get; }
        public Exp Value { get; }
    }

    public record class ReturnStmt(ReturnValueInfo? Info) : Stmt
    {
        public override void Accept<TStmtVisitor>(ref TStmtVisitor visitor) => visitor.VisitReturn(this);
    }

    // Stmt중간에 명시적으로 { }를 사용한 경우에만 BlockStmt로 나타낸다. 함수, for, async, await, if등에 나타나는 { 이후 구문들은 EmbeddableStmt를 사용한다
    public record class BlockStmt(ImmutableArray<Stmt> Stmts) : Stmt
    {
        public override void Accept<TStmtVisitor>(ref TStmtVisitor visitor) => visitor.VisitBlock(this);
    }
    
    public record class BlankStmt : Stmt
    {
        public static readonly BlankStmt Instance = new BlankStmt();
        BlankStmt() { }
        public override void Accept<TStmtVisitor>(ref TStmtVisitor visitor) => visitor.VisitBlank(this);
    }

    public record class ExpStmt(Exp Exp) : Stmt
    {
        public override void Accept<TStmtVisitor>(ref TStmtVisitor visitor) => visitor.VisitExp(this);
    }

    public record class TaskStmt(ImmutableArray<Stmt> Body) : Stmt // task { ... }
    {
        public override void Accept<TStmtVisitor>(ref TStmtVisitor visitor) => visitor.VisitTask(this);
    }

    public record class AwaitStmt(ImmutableArray<Stmt> Body) : Stmt // await { ... }
    {
        public override void Accept<TStmtVisitor>(ref TStmtVisitor visitor) => visitor.VisitAwait(this);
    }

    public record class AsyncStmt(ImmutableArray<Stmt> Body) : Stmt // async { ... }
    {
        public override void Accept<TStmtVisitor>(ref TStmtVisitor visitor) => visitor.VisitAsync(this);
    }

    public record class ForeachStmt(bool IsRef, TypeExp Type, string VarName, Exp Iterator, EmbeddableStmt Body) : Stmt
    {
        public override void Accept<TStmtVisitor>(ref TStmtVisitor visitor) => visitor.VisitForeach(this);
    }

    public record class YieldStmt(Exp Value) : Stmt
    {
        public override void Accept<TStmtVisitor>(ref TStmtVisitor visitor) => visitor.VisitYield(this);
    }

    // Stmt에 사용되는 Directive랑 Decl-Level에서 사용되는 Directive가 다르므로 구분해도 될 것 같다
    public record class DirectiveStmt(string Name, ImmutableArray<Exp> Args) : Stmt
    {
        public override void Accept<TStmtVisitor>(ref TStmtVisitor visitor) => visitor.VisitDirective(this);
    }
}