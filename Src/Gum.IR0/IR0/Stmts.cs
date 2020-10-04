using Gum.CompileTime;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Xml.XPath;

namespace Gum.IR0
{
    public abstract class Stmt
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

    // 글로벌 변수 선언
    public class PrivateGlobalVarDeclStmt : Stmt
    {
        public class Element
        {
            public string Name { get; }
            public TypeValue Type { get; }

            public IR0.Exp? InitExp { get; }            
            
            public Element(string name, TypeValue type, IR0.Exp? initExp)
            {
                Name = name;
                Type = type;
                InitExp = initExp;
            }
        }

        public ImmutableArray<Element> Elems { get; }

        public PrivateGlobalVarDeclStmt(IEnumerable<Element> elems)
        {
            Elems = elems.ToImmutableArray();
        }
    }   
    
    public class LocalVarDeclStmt : Stmt
    {
        public LocalVarDecl VarDecl { get; }
        public LocalVarDeclStmt(LocalVarDecl varDecl) { VarDecl = varDecl; }
    }

    public class IfStmt : Stmt
    {
        public Exp Cond { get; }
        public Stmt Body { get; }
        public Stmt? ElseBody { get; }

        public IfStmt(Exp cond, Stmt body, Stmt? elseBody)
        {
            Cond = cond;
            Body = body;
            ElseBody = elseBody;
        }        
    }

    public class IfTestClassStmt : Stmt
    {
        public Exp Target { get; }
        public TypeValue TargetType { get; }
        public TypeValue TestType { get; }
        public Stmt Body { get; }
        public Stmt? ElseBody { get; }

        public IfTestClassStmt(Exp target, TypeValue targetType, TypeValue testType, Stmt body, Stmt? elseBody)
        {
            Target = target;
            TargetType = targetType;
            TestType = testType;
            Body = body;
            ElseBody = elseBody;
        }
    }

    public class IfTestEnumStmt : Stmt
    {
        public Exp Target { get; }
        public TypeValue TargetType { get; }
        public string ElemName { get; }
        public Stmt Body { get; }
        public Stmt? ElseBody { get; }        

        public IfTestEnumStmt(Exp target, TypeValue targetType, string elemName, Stmt body, Stmt? elseBody)
        {
            Target = target;
            TargetType = targetType;
            ElemName = elemName;
            Body = body;
            ElseBody = elseBody;
        }
    }
    
    public class ForStmt : Stmt
    {
        // InitExp Or VarDecl
        public ForStmtInitializer? Initializer { get; }
        public Exp? CondExp { get; }
        public ExpAndType? ContinueExpInfo { get; }

        public Stmt Body { get; }

        public ForStmt(ForStmtInitializer? initializer, Exp? condExp, ExpAndType? continueExpInfo, Stmt bodyStmt)
        {
            Initializer = initializer;
            CondExp = condExp;
            ContinueExpInfo = continueExpInfo;
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
        public TypeValue ExpType { get; }

        public ExpStmt(Exp exp, TypeValue expType)
        {
            Exp = exp;
            ExpType = expType;
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
        public CaptureInfo CaptureInfo { get; }

        public TaskStmt(Stmt body, CaptureInfo captureInfo) 
        { 
            Body = body;
            CaptureInfo = captureInfo;
        }
    }

    public class AwaitStmt : Stmt
    {
        public Stmt Body { get; }
        public AwaitStmt(Stmt body) { Body = body; }
    }

    public class AsyncStmt : Stmt
    {
        public Stmt Body { get; }
        public CaptureInfo CaptureInfo { get; }

        public AsyncStmt(Stmt body, CaptureInfo captureInfo) { Body = body; CaptureInfo = captureInfo; }
    }

    public class ForeachStmt : Stmt
    {
        public TypeValue ElemType { get; }
        public string ElemName { get; }

        public Exp Obj { get; }
        public TypeValue ObjType { get; }
        public TypeValue EnumeratorType { get; }        
        
        public FuncValue GetEnumerator { get; }
        public FuncValue MoveNext { get; }
        public FuncValue GetCurrent { get; }

        public Stmt Body { get; }

        public ForeachStmt(
            TypeValue elemType, 
            string elemName, 
            Exp obj, 
            TypeValue objType, 
            TypeValue enumeratorType, 
            FuncValue getEnumerator, 
            FuncValue moveNext, 
            FuncValue getCurrent,
            Stmt body)
        {
            ElemType = elemType;
            ElemName = elemName;
            Obj = obj;
            ObjType = objType;
            EnumeratorType = enumeratorType;
            GetEnumerator = getEnumerator;
            MoveNext = moveNext;
            GetCurrent = getCurrent;
            Body = body;
        }
    }

    public class YieldStmt : Stmt
    {
        public Exp Value { get; }
        public YieldStmt(Exp value) { Value = value; }
    }
}