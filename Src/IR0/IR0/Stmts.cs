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
    public abstract record class Stmt : INode;

    public record CommandStmt(ImmutableArray<StringExp> Commands) : Stmt;    

    // 로컬 변수는 
    public record class LocalVarDeclStmt(IType Type, string Name, Exp InitExp) : Stmt;    // var x = 0;
    public record class LocalRefVarDeclStmt(string Name, Loc Loc) : Stmt; // ref int x = y;

    public record class IfStmt(Exp Cond, ImmutableArray<Stmt> Body, ImmutableArray<Stmt> ElseBody) : Stmt;
    public record class IfTestClassStmt(Loc Target, ClassSymbol Class, Name VarName, ImmutableArray<Stmt> Body, ImmutableArray<Stmt> ElseBody) : Stmt;
    public record class IfTestEnumElemStmt(Loc Target, EnumElemSymbol EnumElem, string? VarName, ImmutableArray<Stmt> Body, ImmutableArray<Stmt> ElseBody) : Stmt;
    public record class ForStmt(ImmutableArray<Stmt> InitStmts, Exp? CondExp, Exp? ContinueExp, ImmutableArray<Stmt> Body) : Stmt;

    // singleton
    public record ContinueStmt : Stmt
    {
        public static readonly ContinueStmt Instance = new ContinueStmt();
        ContinueStmt() { }        
    }

    // singleton
    public record class BreakStmt : Stmt
    {
        public static readonly BreakStmt Instance = new BreakStmt();
        BreakStmt() { }        
    }

    public abstract record class ReturnInfo
    {
        public record class Ref(Loc Loc) : ReturnInfo;
        public record class Expression(Exp Exp) : ReturnInfo;
        public record class None : ReturnInfo
        {
            public static readonly None Instance = new None();
            None() { }
        }
    }
    
    public record class ReturnStmt(ReturnInfo Info): Stmt;    
    public record class BlockStmt(ImmutableArray<Stmt> Stmts) : Stmt;
    public record class BlankStmt : Stmt;
    public record class ExpStmt(Exp Exp) : Stmt;
    public record class TaskStmt(LambdaSymbol Lambda, ImmutableArray<Argument> CaptureArgs, ImmutableArray<Stmt> Body) : Stmt;
    public record class AwaitStmt(ImmutableArray<Stmt> Body) : Stmt;
    
    public record class AsyncStmt(LambdaSymbol Lambda, ImmutableArray<Argument> CaptureArgs, ImmutableArray<Stmt> Body) : Stmt;
    
    public record class ForeachStmt(IType ItemType, string ElemName, Loc Iterator, ImmutableArray<Stmt> Body) : Stmt;
    public record class YieldStmt(Exp Value) : Stmt;

    // Constructor 내에서 상위 Constructor 호출시 사용
    public record CallClassConstructorStmt(ClassConstructorSymbol Constructor, ImmutableArray<Argument> Args) : Stmt;
    public record CallStructConstructorStmt(StructConstructorSymbol Constructor, ImmutableArray<Argument> Args) : Stmt;

    public abstract record class DirectiveStmt : Stmt
    {
        // init은 지원하지 않기로,
        // public record class Init(ImmutableArray<Loc> Locs) : DirectiveStmt;        
        public record class Null(Loc Loc) : DirectiveStmt;
        public record class NotNull(Loc Loc) : DirectiveStmt;

        public record class StaticNull(Loc Loc) : DirectiveStmt;   // 컴파일러 스태틱 체크
        public record class StaticNotNull(Loc Loc) : DirectiveStmt;
        public record class StaticUnknownNull(Loc Loc) : DirectiveStmt;
    }    
}
