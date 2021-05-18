using Gum.Syntax;
using System;
using System.Collections.Generic;
using Gum.Collections;
using System.Diagnostics;
using System.Linq;
using System.Xml.XPath;

namespace Gum.Syntax
{
    public abstract record Stmt : ISyntaxNode;
    
    // 명령어
    // TODO: commands의 Length가 1인 contract를 추가하자
    public record CommandStmt(ImmutableArray<StringExp> commands) : Stmt;

    // int a = 0, b, c;
    public record VarDeclStmt(VarDecl VarDecl) : Stmt;    
    public record IfStmt(Exp Cond, TypeExp? TestType, Stmt Body, Stmt? ElseBody) : Stmt;

    public record ForStmt(ForStmtInitializer? Initializer, Exp? CondExp, Exp? ContinueExp, Stmt Body) : Stmt;    
    public record ContinueStmt : Stmt
    {
        public static readonly ContinueStmt Instance = new ContinueStmt();
        ContinueStmt() { }
    }

    public record BreakStmt : Stmt
    {
        public static readonly BreakStmt Instance = new BreakStmt();
        BreakStmt() { }
    }

    public record ReturnStmt : Stmt
    {
        public Exp? Value { get; }
        public ReturnStmt(Exp? value) { Value = value; }
    }

    public record  BlockStmt(ImmutableArray<Stmt> Stmts) : Stmt;
    
    public record BlankStmt : Stmt
    {
        public static readonly BlankStmt Instance = new BlankStmt();
        BlankStmt() { }
    }

    public class ExpStmt : Stmt
    {
        public Exp Exp { get; }
        public ExpStmt(Exp exp)
        {
            Exp = exp;
        }
    }

    public class TaskStmt : Stmt
    {
        public Stmt Body { get; }
        public TaskStmt(Stmt body) { Body = body; }
    }

    public class AwaitStmt : Stmt
    {
        public Stmt Body { get; }
        public AwaitStmt(Stmt body) { Body = body; }
    }

    public class AsyncStmt : Stmt
    {
        public Stmt Body { get; }
        public AsyncStmt(Stmt body) { Body = body; }
    }

    public class ForeachStmt : Stmt
    {
        public TypeExp Type { get; }
        public string VarName { get; }
        public Exp Iterator { get; }
        public Stmt Body { get; }

        public ForeachStmt(TypeExp type, string varName, Exp iterator, Stmt body)
        {
            Type = type;
            VarName = varName;
            Iterator = iterator;
            Body = body;
        }       
    }

    public class YieldStmt : Stmt
    {
        public Exp Value { get; }
        public YieldStmt(Exp value) { Value = value; }
    }
}