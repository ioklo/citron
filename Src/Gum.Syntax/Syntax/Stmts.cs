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

        public override bool Equals(object? obj)
        {
            return obj is CommandStmt statement &&
                   Enumerable.SequenceEqual(Commands, statement.Commands);
        }

        public override int GetHashCode()
        {
            var hashCode = new HashCode();

            foreach (var command in Commands)
                hashCode.Add(command);

            return hashCode.ToHashCode();
        }

        public static bool operator ==(CommandStmt? left, CommandStmt? right)
        {
            return EqualityComparer<CommandStmt?>.Default.Equals(left, right);
        }

        public static bool operator !=(CommandStmt? left, CommandStmt? right)
        {
            return !(left == right);
        }
    }

    public struct VarDeclElement : ISyntaxNode
    {
        public string VarName { get; }
        public Exp? InitExp { get; }

        public VarDeclElement(string varName, Exp? initExp)
        {
            VarName = varName;
            InitExp = initExp;
        }

        public override bool Equals(object? obj)
        {
            return obj is VarDeclElement element &&
                   VarName == element.VarName &&
                   EqualityComparer<Exp?>.Default.Equals(InitExp, element.InitExp);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(VarName, InitExp);
        }

        public static bool operator ==(VarDeclElement left, VarDeclElement right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(VarDeclElement left, VarDeclElement right)
        {
            return !(left == right);
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

        public override bool Equals(object? obj)
        {
            return obj is VarDecl decl &&
                   EqualityComparer<TypeExp>.Default.Equals(Type, decl.Type) &&
                   Enumerable.SequenceEqual(Elems, decl.Elems);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Type, Elems);
        }

        public static bool operator ==(VarDecl? left, VarDecl? right)
        {
            return EqualityComparer<VarDecl?>.Default.Equals(left, right);
        }

        public static bool operator !=(VarDecl? left, VarDecl? right)
        {
            return !(left == right);
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

        public override bool Equals(object? obj)
        {
            return obj is VarDeclStmt stmt &&
                   EqualityComparer<VarDecl>.Default.Equals(VarDecl, stmt.VarDecl);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(VarDecl);
        }

        public static bool operator ==(VarDeclStmt? left, VarDeclStmt? right)
        {
            return EqualityComparer<VarDeclStmt?>.Default.Equals(left, right);
        }

        public static bool operator !=(VarDeclStmt? left, VarDeclStmt? right)
        {
            return !(left == right);
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

        public override bool Equals(object? obj)
        {
            return obj is IfStmt stmt &&
                   EqualityComparer<Exp>.Default.Equals(Cond, stmt.Cond) &&
                   EqualityComparer<TypeExp?>.Default.Equals(TestType, stmt.TestType) &&
                   EqualityComparer<Stmt>.Default.Equals(Body, stmt.Body) &&
                   EqualityComparer<Stmt?>.Default.Equals(ElseBody, stmt.ElseBody);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Cond, Body, ElseBody);
        }

        public static bool operator ==(IfStmt? left, IfStmt? right)
        {
            return EqualityComparer<IfStmt?>.Default.Equals(left, right);
        }

        public static bool operator !=(IfStmt? left, IfStmt? right)
        {
            return !(left == right);
        }
    }

    
    public abstract class ForStmtInitializer : ISyntaxNode{ }
    public class ExpForStmtInitializer : ForStmtInitializer
    {
        public Exp Exp { get; }
        public ExpForStmtInitializer(Exp exp) { Exp = exp; }

        public override bool Equals(object? obj)
        {
            return obj is ExpForStmtInitializer initializer &&
                   EqualityComparer<Exp>.Default.Equals(Exp, initializer.Exp);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Exp);
        }

        public static bool operator ==(ExpForStmtInitializer? left, ExpForStmtInitializer? right)
        {
            return EqualityComparer<ExpForStmtInitializer?>.Default.Equals(left, right);
        }

        public static bool operator !=(ExpForStmtInitializer? left, ExpForStmtInitializer? right)
        {
            return !(left == right);
        }
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

        public override bool Equals(object? obj)
        {
            return obj is ForStmt stmt &&
                   EqualityComparer<ForStmtInitializer?>.Default.Equals(Initializer, stmt.Initializer) &&
                   EqualityComparer<Exp?>.Default.Equals(CondExp, stmt.CondExp) &&
                   EqualityComparer<Exp?>.Default.Equals(ContinueExp, stmt.ContinueExp) &&
                   EqualityComparer<Stmt>.Default.Equals(Body, stmt.Body);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Initializer, CondExp, ContinueExp, Body);
        }

        public static bool operator ==(ForStmt? left, ForStmt? right)
        {
            return EqualityComparer<ForStmt?>.Default.Equals(left, right);
        }

        public static bool operator !=(ForStmt? left, ForStmt? right)
        {
            return !(left == right);
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

        public override bool Equals(object? obj)
        {
            return obj is ReturnStmt stmt &&
                   EqualityComparer<Exp?>.Default.Equals(Value, stmt.Value);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Value);
        }

        public static bool operator ==(ReturnStmt? left, ReturnStmt? right)
        {
            return EqualityComparer<ReturnStmt?>.Default.Equals(left, right);
        }

        public static bool operator !=(ReturnStmt? left, ReturnStmt? right)
        {
            return !(left == right);
        }
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

        public override bool Equals(object? obj)
        {
            return obj is BlockStmt stmt && Enumerable.SequenceEqual(Stmts, stmt.Stmts);
        }

        public override int GetHashCode()
        {
            var hashCode = new HashCode();

            foreach (var stmt in Stmts)
                hashCode.Add(stmt);

            return hashCode.ToHashCode();
        }

        public static bool operator ==(BlockStmt? left, BlockStmt? right)
        {
            return EqualityComparer<BlockStmt?>.Default.Equals(left, right);
        }

        public static bool operator !=(BlockStmt? left, BlockStmt? right)
        {
            return !(left == right);
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

        public override bool Equals(object? obj)
        {
            return obj is ExpStmt stmt &&
                   EqualityComparer<Exp>.Default.Equals(Exp, stmt.Exp);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Exp);
        }

        public static bool operator ==(ExpStmt? left, ExpStmt? right)
        {
            return EqualityComparer<ExpStmt?>.Default.Equals(left, right);
        }

        public static bool operator !=(ExpStmt? left, ExpStmt? right)
        {
            return !(left == right);
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
        public Exp Obj { get; }
        public Stmt Body { get; }

        public ForeachStmt(TypeExp type, string varName, Exp obj, Stmt body)
        {
            Type = type;
            VarName = varName;
            Obj = obj;
            Body = body;
        }

        public override bool Equals(object? obj)
        {
            return obj is ForeachStmt stmt &&
                   EqualityComparer<TypeExp>.Default.Equals(Type, stmt.Type) &&
                   VarName == stmt.VarName &&
                   EqualityComparer<Exp>.Default.Equals(Obj, stmt.Obj) &&
                   EqualityComparer<Stmt>.Default.Equals(Body, stmt.Body);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Type, VarName, Obj, Body);
        }

        public static bool operator ==(ForeachStmt? left, ForeachStmt? right)
        {
            return EqualityComparer<ForeachStmt?>.Default.Equals(left, right);
        }

        public static bool operator !=(ForeachStmt? left, ForeachStmt? right)
        {
            return !(left == right);
        }
    }

    public class YieldStmt : Stmt
    {
        public Exp Value { get; }
        public YieldStmt(Exp value) { Value = value; }

        public override bool Equals(object? obj)
        {
            return obj is YieldStmt stmt &&
                   EqualityComparer<Exp>.Default.Equals(Value, stmt.Value);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Value);
        }

        public static bool operator ==(YieldStmt? left, YieldStmt? right)
        {
            return EqualityComparer<YieldStmt?>.Default.Equals(left, right);
        }

        public static bool operator !=(YieldStmt? left, YieldStmt? right)
        {
            return !(left == right);
        }
    }
}