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
            public TypeId TypeId { get; }

            public IR0.Exp? InitExp { get; }            
            
            public Element(string name, TypeId typeId, IR0.Exp? initExp)
            {
                Name = name;
                TypeId = typeId;
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
        public ExpInfo Target { get; }
        public TypeId TestType { get; }
        public Stmt Body { get; }
        public Stmt? ElseBody { get; }

        public IfTestClassStmt(ExpInfo target, TypeId testType, Stmt body, Stmt? elseBody)
        {
            Target = target;
            TestType = testType;
            Body = body;
            ElseBody = elseBody;
        }
    }

    public class IfTestEnumStmt : Stmt
    {
        public ExpInfo Target { get; }
        public string ElemName { get; }
        public Stmt Body { get; }
        public Stmt? ElseBody { get; }        

        public IfTestEnumStmt(ExpInfo target, string elemName, Stmt body, Stmt? elseBody)
        {
            Target = target;
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
        public ExpInfo? ContinueInfo { get; }

        public Stmt Body { get; }

        public ForStmt(ForStmtInitializer? initializer, Exp? condExp, ExpInfo? continueInfo, Stmt bodyStmt)
        {
            Initializer = initializer;
            CondExp = condExp;
            ContinueInfo = continueInfo;
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
        public ExpInfo ExpInfo { get; }

        public ExpStmt(ExpInfo expInfo)
        {
            ExpInfo = expInfo;
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
        public TypeId ElemTypeId { get; set; }
        public string ElemName { get; }

        public ExpInfo ObjInfo { get; }        
        public Stmt Body { get; }

        public ForeachStmt(
            TypeId elemTypeId,
            string elemName, 
            ExpInfo objInfo, 
            Stmt body)
        {
            ElemTypeId = elemTypeId;
            ElemName = elemName;
            ObjInfo = objInfo;
            Body = body;
        }
    }

    public class YieldStmt : Stmt
    {
        public Exp Value { get; }
        public YieldStmt(Exp value) { Value = value; }
    }
}