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
    public abstract class Stmt : IPure
    {
        public void EnsurePure() { }
    }

    // 명령어
    [ImplementIEquatable]
    public partial class CommandStmt : Stmt
    {
        public ImmutableArray<StringExp> Commands { get; }

        public CommandStmt(ImmutableArray<StringExp> commands)
        {
            Debug.Assert(0 < commands.Length);
            Commands = commands;
        }        
    }

    // 글로벌 변수 선언
    [AutoConstructor, ImplementIEquatable]
    public partial class PrivateGlobalVarDeclStmt : Stmt
    {
        public ImmutableArray<VarDeclElement> Elems { get; }
    }

    [AutoConstructor, ImplementIEquatable]
    public partial class LocalVarDeclStmt : Stmt
    {
        public LocalVarDecl VarDecl { get; }
    }

    [AutoConstructor, ImplementIEquatable]
    public partial class IfStmt : Stmt
    {
        public Exp Cond { get; }
        public Stmt Body { get; }
        public Stmt? ElseBody { get; }
    }

    [AutoConstructor, ImplementIEquatable]
    public partial class IfTestClassStmt : Stmt
    {
        public Loc Target { get; }
        public Path TestType { get; } 
        public Stmt Body { get; }
        public Stmt? ElseBody { get; }
    }

    [AutoConstructor, ImplementIEquatable]
    public partial class IfTestEnumStmt : Stmt
    {
        public Loc Target { get; }
        public string ElemName { get; }
        public Stmt Body { get; }
        public Stmt? ElseBody { get; }
    }

    [AutoConstructor, ImplementIEquatable]
    public partial class ForStmt : Stmt
    {
        // InitExp Or VarDecl
        public ForStmtInitializer? Initializer { get; }
        public Exp? CondExp { get; }
        public Exp? ContinueExp { get; }

        public Stmt Body { get; }
    }

    public partial class ContinueStmt : Stmt
    {
        public static readonly ContinueStmt Instance = new ContinueStmt();
        private ContinueStmt() { }
    }

    public partial class BreakStmt : Stmt
    {
        public static readonly BreakStmt Instance = new BreakStmt();
        private BreakStmt() { }
    }

    [AutoConstructor, ImplementIEquatable]
    public partial class ReturnStmt : Stmt
    {
        public Exp? Value { get; }
    }

    [AutoConstructor, ImplementIEquatable]
    public partial class BlockStmt : Stmt
    {
        public ImmutableArray<Stmt> Stmts { get; }
    }

    public partial class BlankStmt : Stmt
    {
        public static readonly BlankStmt Instance = new BlankStmt();
        private BlankStmt() { }
    }

    [AutoConstructor, ImplementIEquatable]
    public partial class ExpStmt : Stmt
    {
        public Exp Exp { get; }
    }

    [AutoConstructor, ImplementIEquatable]
    public partial class TaskStmt : Stmt
    {
        public Path.Nested CapturedStatementDecl { get; }
    }

    [AutoConstructor, ImplementIEquatable]
    public partial class AwaitStmt : Stmt
    {
        public Stmt Body { get; }
    }

    [AutoConstructor, ImplementIEquatable]
    public partial class AsyncStmt : Stmt
    {
        public Path.Nested CapturedStatementDecl { get; }
    }

    [AutoConstructor, ImplementIEquatable]
    public partial class ForeachStmt : Stmt
    {
        public Path ElemType { get; set; }
        public string ElemName { get; }

        public Loc Iterator { get; }        
        public Stmt Body { get; }
    }

    [AutoConstructor, ImplementIEquatable]
    public partial class YieldStmt : Stmt
    {
        public Exp Value { get; }
    }
}