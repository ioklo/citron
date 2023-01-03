﻿using Citron.Syntax;
using System;
using System.Collections.Generic;
using Citron.Collections;
using System.Diagnostics;
using System.Linq;
using System.Xml.XPath;
using Pretune;

namespace Citron.Syntax
{
    public abstract record class Stmt : ISyntaxNode;
    
    // 명령어
    // TODO: commands의 Length가 1인 contract를 추가하자
    public record CommandStmt(ImmutableArray<StringExp> Commands) : Stmt;

    // int a = 0, b, c;
    public record class VarDeclStmt(VarDecl VarDecl) : Stmt;    

    // if ($cond) $body else $ElseBody
    public record class IfStmt(Exp Cond, Stmt Body, Stmt? ElseBody) : Stmt;
    public record class IfTestStmt(Exp Exp, TypeExp TestType, string? VarName, Stmt Body, Stmt? ElseBody) : Stmt;

    public record class ForStmt(ForStmtInitializer? Initializer, Exp? CondExp, Exp? ContinueExp, Stmt Body) : Stmt;    
    public record ContinueStmt : Stmt
    {
        public static readonly ContinueStmt Instance = new ContinueStmt();
        ContinueStmt() { }
    }

    public record class BreakStmt : Stmt
    {
        public static readonly BreakStmt Instance = new BreakStmt();
        BreakStmt() { }
    }

    [AutoConstructor, ImplementIEquatable]
    public partial struct ReturnValueInfo
    {
        public bool IsRef { get; }
        public Exp Value { get; }
    }

    public record class ReturnStmt(ReturnValueInfo? Info) : Stmt;

    public record class BlockStmt(ImmutableArray<Stmt> Stmts) : Stmt;
    
    public record class BlankStmt : Stmt
    {
        public static readonly BlankStmt Instance = new BlankStmt();
        BlankStmt() { }
    }

    public record class ExpStmt(Exp Exp) : Stmt;    
    public record class TaskStmt(Stmt Body) : Stmt;
    public record class AwaitStmt(Stmt Body) : Stmt;    
    public record class AsyncStmt(Stmt Body) : Stmt;
    public record class ForeachStmt(bool IsRef, TypeExp Type, string VarName, Exp Iterator, Stmt Body) : Stmt;
    public record class YieldStmt(Exp Value) : Stmt;    

    // Stmt에 사용되는 Directive랑 Decl-Level에서 사용되는 Directive가 다르므로 구분해도 될 것 같다
    public record class DirectiveStmt(string Name, ImmutableArray<Exp> Args) : Stmt;
}