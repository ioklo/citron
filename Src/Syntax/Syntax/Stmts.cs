using Citron.Syntax;
using System;
using System.Collections.Generic;
using Citron.Collections;
using System.Diagnostics;
using System.Linq;
using System.Xml.XPath;
using System.Data;

namespace Citron.Syntax
{
    public abstract record class Stmt : ISyntaxNode
    {
        public abstract TResult Accept<TVisitor, TResult>(ref TVisitor visitor)
            where TVisitor : struct, IStmtVisitor<TResult>;
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
        public override TResult Accept<TVisitor, TResult>(ref TVisitor visitor) => visitor.VisitCommand(this);
    }

    // int a = 0, b, c;
    public record class VarDeclStmt(VarDecl VarDecl) : Stmt
    {
        public override TResult Accept<TVisitor, TResult>(ref TVisitor visitor) => visitor.VisitVarDecl(this);
    }

    // if ($cond) $body else $ElseBody
    public record class IfStmt(Exp Cond, EmbeddableStmt Body, EmbeddableStmt? ElseBody) : Stmt
    {
        public override TResult Accept<TVisitor, TResult>(ref TVisitor visitor) => visitor.VisitIf(this);
    }

    // if (testType varName = exp
    public record class IfTestStmt(TypeExp TestTypeExp, string VarName, Exp Exp, EmbeddableStmt Body, EmbeddableStmt? ElseBody) : Stmt
    {
        public override TResult Accept<TVisitor, TResult>(ref TVisitor visitor) => visitor.VisitIfTest(this);
    }

    public record class ForStmt(ForStmtInitializer? Initializer, Exp? CondExp, Exp? ContinueExp, EmbeddableStmt Body) : Stmt
    {
        public override TResult Accept<TVisitor, TResult>(ref TVisitor visitor) => visitor.VisitFor(this);
    }

    public record ContinueStmt : Stmt
    {   
        public override TResult Accept<TVisitor, TResult>(ref TVisitor visitor) => visitor.VisitContinue(this);
    }

    public record class BreakStmt : Stmt
    {   
        public override TResult Accept<TVisitor, TResult>(ref TVisitor visitor) => visitor.VisitBreak(this);
    }
    
    public record struct ReturnValueInfo(Exp Value);

    public record class ReturnStmt(ReturnValueInfo? Info) : Stmt
    {
        public override TResult Accept<TVisitor, TResult>(ref TVisitor visitor) => visitor.VisitReturn(this);
    }

    // Stmt중간에 명시적으로 { }를 사용한 경우에만 BlockStmt로 나타낸다. 함수, for, async, await, if등에 나타나는 { 이후 구문들은 EmbeddableStmt를 사용한다
    public record class BlockStmt(ImmutableArray<Stmt> Stmts) : Stmt
    {
        public override TResult Accept<TVisitor, TResult>(ref TVisitor visitor) => visitor.VisitBlock(this);
    }
    
    public record class BlankStmt : Stmt
    {        
        public override TResult Accept<TVisitor, TResult>(ref TVisitor visitor) => visitor.VisitBlank(this);
    }

    public record class ExpStmt(Exp Exp) : Stmt
    {
        public override TResult Accept<TVisitor, TResult>(ref TVisitor visitor) => visitor.VisitExp(this);
    }

    public record class TaskStmt(ImmutableArray<Stmt> Body) : Stmt // task { ... }
    {
        public override TResult Accept<TVisitor, TResult>(ref TVisitor visitor) => visitor.VisitTask(this);
    }

    public record class AwaitStmt(ImmutableArray<Stmt> Body) : Stmt // await { ... }
    {
        public override TResult Accept<TVisitor, TResult>(ref TVisitor visitor) => visitor.VisitAwait(this);
    }

    public record class AsyncStmt(ImmutableArray<Stmt> Body) : Stmt // async { ... }
    {
        public override TResult Accept<TVisitor, TResult>(ref TVisitor visitor) => visitor.VisitAsync(this);
    }

    public record class ForeachStmt(TypeExp Type, string VarName, Exp Enumerable, EmbeddableStmt Body) : Stmt
    {
        public override TResult Accept<TVisitor, TResult>(ref TVisitor visitor) => visitor.VisitForeach(this);
    }

    public record class YieldStmt(Exp Value) : Stmt
    {
        public override TResult Accept<TVisitor, TResult>(ref TVisitor visitor) => visitor.VisitYield(this);
    }

    // Stmt에 사용되는 Directive랑 Decl-Level에서 사용되는 Directive가 다르므로 구분해도 될 것 같다
    public record class DirectiveStmt(string Name, ImmutableArray<Exp> Args) : Stmt
    {
        public override TResult Accept<TVisitor, TResult>(ref TVisitor visitor) => visitor.VisitDirective(this);
    }
}