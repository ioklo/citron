using Gum.Syntax;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Xml.XPath;

namespace Gum.Syntax
{
    public abstract class Stmt : ISyntaxNode
    {
    }
    
    // 명령어
    public class CommandStmt : Stmt
    {
        public ImmutableArray<StringExp> Commands { get; }

        public CommandStmt(IEnumerable<StringExp> commands)
        {
            Debug.Assert(0 < commands.Count());
            Commands = commands.ToImmutableArray();
        }

        public CommandStmt(params StringExp[] commands)
        {
            Debug.Assert(0 < commands.Length);
            Commands = ImmutableArray.Create(commands);
        }
    }

    public class VarDeclElement : ISyntaxNode
    {
        public string VarName { get; }
        public Exp? InitExp { get; }

        public VarDeclElement(string varName, Exp? initExp)
        {
            VarName = varName;
            InitExp = initExp;
        }
    }

    public class VarDecl : ISyntaxNode
    {
        public TypeExp Type { get; }
        public ImmutableArray<VarDeclElement> Elems { get; }

        public VarDecl(TypeExp type, IEnumerable<VarDeclElement> elems)
        {
            Type = type;
            Elems = elems.ToImmutableArray();
        }

        public VarDecl(TypeExp type, params VarDeclElement[] elems)
        {
            Type = type;
            Elems = ImmutableArray.Create(elems);
        }
    }

    // int a = 0, b, c;
    public class VarDeclStmt : Stmt
    {
        public VarDecl VarDecl { get; }

        public VarDeclStmt(VarDecl varDecl)
        {
            VarDecl = varDecl;
        }
    }

    public class IfStmt : Stmt
    {
        public Exp Cond { get; }
        public TypeExp? TestType { get; }
        public Stmt Body { get; }
        public Stmt? ElseBody { get; }

        public IfStmt(Exp cond, TypeExp? testType, Stmt body, Stmt? elseBody)
        {
            Cond = cond;
            TestType = testType;
            Body = body;
            ElseBody = elseBody;
        }
    }

    
    public abstract class ForStmtInitializer : ISyntaxNode{ }
    public class ExpForStmtInitializer : ForStmtInitializer
    {
        public Exp Exp { get; }
        public ExpForStmtInitializer(Exp exp) { Exp = exp; }
    }

    public class VarDeclForStmtInitializer : ForStmtInitializer
    {
        public VarDecl VarDecl { get; }
        public VarDeclForStmtInitializer(VarDecl varDecl) { VarDecl = varDecl; }

        public override bool Equals(object? obj)
        {
            return obj is VarDeclForStmtInitializer initializer &&
                   EqualityComparer<VarDecl>.Default.Equals(VarDecl, initializer.VarDecl);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(VarDecl);
        }

        public static bool operator ==(VarDeclForStmtInitializer? left, VarDeclForStmtInitializer? right)
        {
            return EqualityComparer<VarDeclForStmtInitializer?>.Default.Equals(left, right);
        }

        public static bool operator !=(VarDeclForStmtInitializer? left, VarDeclForStmtInitializer? right)
        {
            return !(left == right);
        }
    }

    public class ForStmt : Stmt
    {
        // InitExp Or VarDecl
        public ForStmtInitializer? Initializer { get; }
        public Exp? CondExp { get; }
        public Exp? ContinueExp { get; }
        public Stmt Body { get; }

        public ForStmt(ForStmtInitializer? initializer, Exp? condExp, Exp? continueExp, Stmt bodyStmt)
        {
            Initializer = initializer;
            CondExp = condExp;
            ContinueExp = continueExp;
            Body = bodyStmt;
        }
    }

    public class ContinueStmt : Stmt
    {
        public static ContinueStmt Instance { get; } = new ContinueStmt();
        private ContinueStmt() { }
    }

    public class BreakStmt : Stmt
    {
        public static BreakStmt Instance { get; } = new BreakStmt();
        private BreakStmt() { }
    }

    public class ReturnStmt : Stmt
    {
        public Exp? Value { get; }
        public ReturnStmt(Exp? value) { Value = value; }
    }

    public class BlockStmt : Stmt
    {
        public ImmutableArray<Stmt> Stmts { get; }
        public BlockStmt(IEnumerable<Stmt> stmts)
        {
            Stmts = stmts.ToImmutableArray();
        }

        public BlockStmt(params Stmt[] stmts)
        {
            Stmts = ImmutableArray.Create(stmts);
        }
    }

    public class BlankStmt : Stmt
    {
        public static BlankStmt Instance { get; } = new BlankStmt();
        private BlankStmt() { }
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