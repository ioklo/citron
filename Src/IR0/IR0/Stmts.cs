﻿using Citron.Module;
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
    }

    public record CommandStmt(ImmutableArray<StringExp> Commands) : Stmt;    

    // 글로벌 변수 선언
    public record class GlobalVarDeclStmt(ImmutableArray<VarDeclElement> Elems) : Stmt;
    public record class LocalVarDeclStmt(LocalVarDecl VarDecl) : Stmt;
    public record class IfStmt(Exp Cond, Stmt Body, Stmt? ElseBody) : Stmt;
    public record class IfTestClassStmt(Loc Target, ClassSymbol Class, Name VarName, Stmt Body, Stmt? ElseBody) : Stmt;
    public record class IfTestEnumElemStmt(Loc Target, EnumElemSymbol EnumElem, string? VarName, Stmt Body, Stmt? ElseBody) : Stmt;
    public record class ForStmt(ForStmtInitializer? Initializer, Exp? CondExp, Exp? ContinueExp, Stmt Body) : Stmt;
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
    public record class AwaitStmt(Stmt Body) : Stmt;
    
    public record class AsyncStmt(LambdaSymbol Lambda, ImmutableArray<Argument> CaptureArgs, ImmutableArray<Stmt> Body) : Stmt;
    
    public record class ForeachStmt(ITypeSymbol ElemType, string ElemName, Loc Iterator, Stmt Body) : Stmt;
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
