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
    public abstract record Stmt : IPure
    {
        public void EnsurePure() { }
    }

    public record CommandStmt(ImmutableArray<StringExp> Commands) : Stmt;

    // 글로벌 변수 선언
    public record GlobalVarDeclStmt(ImmutableArray<VarDeclElement> Elems) : Stmt;
    public record GlobalRefVarDeclStmt(ImmutableArray<RefVarDeclElement> Elems) : Stmt;

    public record LocalVarDeclStmt(LocalVarDecl VarDecl) : Stmt;    
    public record LocalRefVarDeclStmt(LocalRefVarDecl RefVarDecl) : Stmt;

    public record IfStmt(Exp Cond, Stmt Body, Stmt? ElseBody) : Stmt;    
    public record IfTestClassStmt(Loc Target, Path TestType, Name VarName, Stmt Body, Stmt? ElseBody) : Stmt;
    public record IfTestEnumElemStmt(Loc Target, Path.Nested EnumElem, string? VarName, Stmt Body, Stmt? ElseBody) : Stmt;    
    public record ForStmt(ForStmtInitializer? Initializer, Exp? CondExp, Exp? ContinueExp, Stmt Body) : Stmt;


    // singleton
    public record ContinueStmt : Stmt
    {
        public static readonly ContinueStmt Instance = new ContinueStmt();
        ContinueStmt() { }
    }

    // singleton
    public record BreakStmt : Stmt
    {
        public static readonly BreakStmt Instance = new BreakStmt();
        BreakStmt() { }
    }
    
    public record ReturnStmt(Exp? Value): Stmt;    
    public record BlockStmt(ImmutableArray<Stmt> Stmts) : Stmt;

    // singleton
    public record BlankStmt : Stmt
    {
        public static readonly BlankStmt Instance = new BlankStmt();
        BlankStmt() { }
    }
    
    public record ExpStmt(Exp Exp) : Stmt;
    public record TaskStmt(Path.Nested CapturedStatementDecl) : Stmt;    
    public record AwaitStmt(Stmt Body) : Stmt;
    public record AsyncStmt(Path.Nested CapturedStatementDecl) : Stmt;    
    public record ForeachStmt(Path ElemType, string ElemName, Loc Iterator, Stmt Body) : Stmt;    
    public record YieldStmt(Exp Value) : Stmt;
}