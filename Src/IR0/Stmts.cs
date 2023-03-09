using Pretune;
using System;
using System.Collections.Generic;
using Citron.Collections;
using System.Diagnostics;
using System.Linq;
using Citron.Infra;
using Citron.Symbol;

namespace Citron.IR0
{   
    public abstract record class Stmt : INode
    {
        public abstract void Accept<TVisitor>(ref TVisitor visitor)
            where TVisitor : struct, IIR0StmtVisitor;
    }

    public record CommandStmt(ImmutableArray<StringExp> Commands) : Stmt
    {
        public override void Accept<TVisitor>(ref TVisitor visitor) => visitor.VisitCommand(this);
    }

    // 로컬 변수는 
    public record class LocalVarDeclStmt(IType Type, string Name, Exp? InitExp) : Stmt    // var x = 0;
    {
        public override void Accept<TVisitor>(ref TVisitor visitor) => visitor.VisitLocalVarDecl(this);
    }

    public record class LocalRefVarDeclStmt(string Name, Loc Loc) : Stmt // ref int x = y;
    {
        public override void Accept<TVisitor>(ref TVisitor visitor) => visitor.VisitLocalRefVarDecl(this);
    }

    public record class IfStmt(Exp Cond, ImmutableArray<Stmt> Body, ImmutableArray<Stmt> ElseBody) : Stmt
    {
        public override void Accept<TVisitor>(ref TVisitor visitor) => visitor.VisitIf(this);
    }

    public record class IfTestClassStmt(Loc Target, ClassSymbol Class, Name VarName, ImmutableArray<Stmt> Body, ImmutableArray<Stmt> ElseBody) : Stmt
    {
        public override void Accept<TVisitor>(ref TVisitor visitor) => visitor.VisitIfTestClass(this);
    }

    public record class IfTestEnumElemStmt(Loc Target, EnumElemSymbol EnumElem, string? VarName, ImmutableArray<Stmt> Body, ImmutableArray<Stmt> ElseBody) : Stmt
    {
        public override void Accept<TVisitor>(ref TVisitor visitor) => visitor.VisitIfTestEnumElem(this);
    }

    public record class ForStmt(ImmutableArray<Stmt> InitStmts, Exp? CondExp, Exp? ContinueExp, ImmutableArray<Stmt> Body) : Stmt
    {
        public override void Accept<TVisitor>(ref TVisitor visitor) => visitor.VisitFor(this);
    }
    
    public record class ContinueStmt() : Stmt
    {
        public override void Accept<TVisitor>(ref TVisitor visitor) => visitor.VisitContinue(this);
    }

    public record class BreakStmt() : Stmt
    {
        public override void Accept<TVisitor>(ref TVisitor visitor) => visitor.VisitBreak(this);
    }

    public abstract record class ReturnInfo
    {
        public record class Ref(Loc Loc) : ReturnInfo;
        public record class Expression(Exp Exp) : ReturnInfo;
        public record class None : ReturnInfo;
    }
    
    public record class ReturnStmt(ReturnInfo Info): Stmt
    {
        public override void Accept<TVisitor>(ref TVisitor visitor) => visitor.VisitReturn(this);
    }

    public record class BlockStmt(ImmutableArray<Stmt> Stmts) : Stmt
    {
        public override void Accept<TVisitor>(ref TVisitor visitor) => visitor.VisitBlock(this);
    }

    public record class BlankStmt : Stmt
    {
        public override void Accept<TVisitor>(ref TVisitor visitor) => visitor.VisitBlank(this);
    }

    public record class ExpStmt(Exp Exp) : Stmt
    {
        public override void Accept<TVisitor>(ref TVisitor visitor) => visitor.VisitExp(this);
    }

    public record class TaskStmt(LambdaSymbol Lambda, ImmutableArray<Argument> CaptureArgs) : Stmt
    {
        public override void Accept<TVisitor>(ref TVisitor visitor) => visitor.VisitTask(this);
    }

    public record class AwaitStmt(ImmutableArray<Stmt> Body) : Stmt
    {
        public override void Accept<TVisitor>(ref TVisitor visitor) => visitor.VisitAwait(this);
    }
    
    public record class AsyncStmt(LambdaSymbol Lambda, ImmutableArray<Argument> CaptureArgs) : Stmt
    {
        public override void Accept<TVisitor>(ref TVisitor visitor) => visitor.VisitAsync(this);
    }
    
    public record class ForeachStmt(IType ItemType, string ElemName, Loc Iterator, ImmutableArray<Stmt> Body) : Stmt
    {
        public override void Accept<TVisitor>(ref TVisitor visitor) => visitor.VisitForeach(this);
    }

    public record class YieldStmt(Exp Value) : Stmt
    {
        public override void Accept<TVisitor>(ref TVisitor visitor) => visitor.VisitYield(this);
    }

    // Constructor 내에서 상위 Constructor 호출시 사용
    public record CallClassConstructorStmt(ClassConstructorSymbol Constructor, ImmutableArray<Argument> Args) : Stmt
    {
        public override void Accept<TVisitor>(ref TVisitor visitor) => visitor.VisitCallClassConstructor(this);
    }

    public record CallStructConstructorStmt(StructConstructorSymbol Constructor, ImmutableArray<Argument> Args) : Stmt
    {
        public override void Accept<TVisitor>(ref TVisitor visitor) => visitor.VisitCallStructConstructor(this);
    }

    public abstract record class DirectiveStmt : Stmt
    {
        public override void Accept<TVisitor>(ref TVisitor visitor) => visitor.VisitDirective(this);

        // init은 지원하지 않기로,
        // public record class Init(ImmutableArray<Loc> Locs) : DirectiveStmt;        
        public record class Null(Loc Loc) : DirectiveStmt;
        public record class NotNull(Loc Loc) : DirectiveStmt;

        public record class StaticNull(Loc Loc) : DirectiveStmt;   // 컴파일러 스태틱 체크
        public record class StaticNotNull(Loc Loc) : DirectiveStmt;
        public record class StaticUnknownNull(Loc Loc) : DirectiveStmt;
    }    
}
