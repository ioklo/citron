using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gum.Lang.AbstractSyntax
{



    public enum UnaryExpKind
    {
        Neg, 
        Not, 
        PrefixInc, 
        PrefixDec, 
        PostfixInc, 
        PostfixDec, 
    }

    public enum BinaryExpKind
    {
        Equal, 
        NotEqual, 
        And, 
        Or, 
        Add, 
        Sub, 
        Mul, 
        Div, 
        Mod, 
        Less, 
        Greater, 
        LessEqual, 
        GreaterEqual, 
    }

    public enum FuncModifier
    {
        Out, 
        Params, 
    }

    public interface IStmtComponent
    {
        void Visit(IStmtComponentVisitor visitor);
    }

    public interface IStmtComponentVisitor
    {
        void Visit(BlockStmt blockStmt);
        void Visit(BreakStmt breakStmt);
        void Visit(ContineStmt contineStmt);
        void Visit(DoWhileStmt doWhileStmt);
        void Visit(ExpStmt expStmt);
        void Visit(ForStmt forStmt);
        void Visit(IfStmt ifStmt);
        void Visit(ReturnStmt returnStmt);
        void Visit(WhileStmt whileStmt);
    }

    public interface IExpComponent
    {
        void Visit(IExpComponentVisitor visitor);
    }

    public interface IExpComponentVisitor
    {
        void Visit(AssignExp assignExp);
        void Visit(BinaryExp binaryExp);
        void Visit(BoolExp boolExp);
        void Visit(CallExp callExp);
        void Visit(FieldExp fieldExp);
        void Visit(IntegerExp integerExp);
        void Visit(NewExp newExp);
        void Visit(StringExp stringExp);
        void Visit(UnaryExp unaryExp);
        void Visit(VariableExp variableExp);
    }

    public interface INamespaceComponent
    {
        void Visit(INamespaceComponentVisitor visitor);
    }

    public interface INamespaceComponentVisitor
    {
        void Visit(VarDecl varDecl);
        void Visit(FuncDecl funcDecl);
        void Visit(ClassDecl classDecl);
        void Visit(StructDecl structDecl);
    }

    public interface IFileUnitComponent
    {
        void Visit(IFileUnitComponentVisitor visitor);
    }

    public interface IFileUnitComponentVisitor
    {
        void Visit(UsingDirective usingDirective);
        void Visit(NamespaceDecl namespaceDecl);
        void Visit(VarDecl varDecl);
        void Visit(FuncDecl funcDecl);
    }

    public class UsingDirective : IFileUnitComponent
    {
        public string NamespaceName { get; private set; }

        public UsingDirective(string namespaceName)
        {
            this.NamespaceName = namespaceName;
        }

        public void Visit(IFileUnitComponentVisitor visitor)
        {
            visitor.Visit(this);
        }
    }

    public class NamespaceDecl : IFileUnitComponent
    {
        public string VarName { get; private set; }
        public IReadOnlyList<INamespaceComponent> Comps { get; private set; }

        public NamespaceDecl(string varName, IEnumerable<INamespaceComponent> comps)
        {
            this.VarName = varName;
            this.Comps = comps.ToList();
        }

        public void Visit(IFileUnitComponentVisitor visitor)
        {
            visitor.Visit(this);
        }
    }

    public class FuncDecl : IFileUnitComponent, INamespaceComponent
    {
        public string ReturnType { get; private set; }
        public string VarName { get; private set; }
        public IReadOnlyList<FuncParam> FuncParams { get; private set; }
        public BlockStmt Body { get; private set; }

        public FuncDecl(string returnType, string varName, IEnumerable<FuncParam> funcParams, BlockStmt body)
        {
            this.ReturnType = returnType;
            this.VarName = varName;
            this.FuncParams = funcParams.ToList();
            this.Body = body;
        }

        public void Visit(IFileUnitComponentVisitor visitor)
        {
            visitor.Visit(this);
        }

        public void Visit(INamespaceComponentVisitor visitor)
        {
            visitor.Visit(this);
        }
    }

    public class VarDecl : IFileUnitComponent, INamespaceComponent
    {
        public string Type { get; private set; }
        public IReadOnlyList<NameAndExp> NameAndExps { get; private set; }

        public VarDecl(string type, IEnumerable<NameAndExp> nameAndExps)
        {
            this.Type = type;
            this.NameAndExps = nameAndExps.ToList();
        }

        public void Visit(IFileUnitComponentVisitor visitor)
        {
            visitor.Visit(this);
        }

        public void Visit(INamespaceComponentVisitor visitor)
        {
            visitor.Visit(this);
        }
    }

    public class ClassDecl : INamespaceComponent
    {

        public ClassDecl()
        {
        }

        public void Visit(INamespaceComponentVisitor visitor)
        {
            visitor.Visit(this);
        }
    }

    public class StructDecl : INamespaceComponent
    {

        public StructDecl()
        {
        }

        public void Visit(INamespaceComponentVisitor visitor)
        {
            visitor.Visit(this);
        }
    }

    public class BlockStmt : IStmtComponent
    {
        public IReadOnlyList<IStmtComponent> Stmts { get; private set; }

        public BlockStmt(IEnumerable<IStmtComponent> stmts)
        {
            this.Stmts = stmts.ToList();
        }

        public void Visit(IStmtComponentVisitor visitor)
        {
            visitor.Visit(this);
        }
    }

    public class BreakStmt : IStmtComponent
    {

        public BreakStmt()
        {
        }

        public void Visit(IStmtComponentVisitor visitor)
        {
            visitor.Visit(this);
        }
    }

    public class ContineStmt : IStmtComponent
    {

        public ContineStmt()
        {
        }

        public void Visit(IStmtComponentVisitor visitor)
        {
            visitor.Visit(this);
        }
    }

    public class DoWhileStmt : IStmtComponent
    {
        public IStmtComponent Body { get; private set; }
        public IExpComponent CondExp { get; private set; }

        public DoWhileStmt(IStmtComponent body, IExpComponent condExp)
        {
            this.Body = body;
            this.CondExp = condExp;
        }

        public void Visit(IStmtComponentVisitor visitor)
        {
            visitor.Visit(this);
        }
    }

    public class ExpStmt : IStmtComponent
    {
        public IExpComponent Exp { get; private set; }

        public ExpStmt(IExpComponent exp)
        {
            this.Exp = exp;
        }

        public void Visit(IStmtComponentVisitor visitor)
        {
            visitor.Visit(this);
        }
    }

    public class ForStmt : IStmtComponent
    {
        public IStmtComponent Initializer { get; private set; }
        public IExpComponent CondExp { get; private set; }
        public IExpComponent LoopExp { get; private set; }
        public IStmtComponent Body { get; private set; }

        public ForStmt(IStmtComponent initializer, IExpComponent condExp, IExpComponent loopExp, IStmtComponent body)
        {
            this.Initializer = initializer;
            this.CondExp = condExp;
            this.LoopExp = loopExp;
            this.Body = body;
        }

        public void Visit(IStmtComponentVisitor visitor)
        {
            visitor.Visit(this);
        }
    }

    public class IfStmt : IStmtComponent
    {
        public IExpComponent CondExp { get; private set; }
        public IStmtComponent ThenStmt { get; private set; }
        public IStmtComponent ElseStmt { get; private set; }

        public IfStmt(IExpComponent condExp, IStmtComponent thenStmt, IStmtComponent elseStmt)
        {
            this.CondExp = condExp;
            this.ThenStmt = thenStmt;
            this.ElseStmt = elseStmt;
        }

        public void Visit(IStmtComponentVisitor visitor)
        {
            visitor.Visit(this);
        }
    }

    public class ReturnStmt : IStmtComponent
    {
        public IExpComponent ReturnExp { get; private set; }

        public ReturnStmt(IExpComponent returnExp)
        {
            this.ReturnExp = returnExp;
        }

        public void Visit(IStmtComponentVisitor visitor)
        {
            visitor.Visit(this);
        }
    }

    public class WhileStmt : IStmtComponent
    {
        public IExpComponent CondExp { get; private set; }
        public IStmtComponent Body { get; private set; }

        public WhileStmt(IExpComponent condExp, IStmtComponent body)
        {
            this.CondExp = condExp;
            this.Body = body;
        }

        public void Visit(IStmtComponentVisitor visitor)
        {
            visitor.Visit(this);
        }
    }

    public class AssignExp : IExpComponent
    {
        public IExpComponent Left { get; private set; }
        public IExpComponent Right { get; private set; }

        public AssignExp(IExpComponent left, IExpComponent right)
        {
            this.Left = left;
            this.Right = right;
        }

        public void Visit(IExpComponentVisitor visitor)
        {
            visitor.Visit(this);
        }
    }

    public class BinaryExp : IExpComponent
    {
        public BinaryExpKind Operation { get; private set; }
        public IExpComponent Operand1 { get; private set; }
        public IExpComponent Operand2 { get; private set; }

        public BinaryExp(BinaryExpKind operation, IExpComponent operand1, IExpComponent operand2)
        {
            this.Operation = operation;
            this.Operand1 = operand1;
            this.Operand2 = operand2;
        }

        public void Visit(IExpComponentVisitor visitor)
        {
            visitor.Visit(this);
        }
    }

    public class BoolExp : IExpComponent
    {
        public bool Value { get; private set; }

        public BoolExp(bool value)
        {
            this.Value = value;
        }

        public void Visit(IExpComponentVisitor visitor)
        {
            visitor.Visit(this);
        }
    }

    public class CallExp : IExpComponent
    {
        public IExpComponent FuncExp { get; private set; }
        public IReadOnlyList<IExpComponent> Args { get; private set; }

        public CallExp(IExpComponent funcExp, IEnumerable<IExpComponent> args)
        {
            this.FuncExp = funcExp;
            this.Args = args.ToList();
        }

        public void Visit(IExpComponentVisitor visitor)
        {
            visitor.Visit(this);
        }
    }

    public class FieldExp : IExpComponent
    {
        public IExpComponent Exp { get; private set; }
        public string ID { get; private set; }

        public FieldExp(IExpComponent exp, string id)
        {
            this.Exp = exp;
            this.ID = id;
        }

        public void Visit(IExpComponentVisitor visitor)
        {
            visitor.Visit(this);
        }
    }

    public class IntegerExp : IExpComponent
    {
        public int Value { get; private set; }

        public IntegerExp(int value)
        {
            this.Value = value;
        }

        public void Visit(IExpComponentVisitor visitor)
        {
            visitor.Visit(this);
        }
    }

    public class NewExp : IExpComponent
    {
        public string Type { get; private set; }
        public IReadOnlyList<string> TypeArgs { get; private set; }
        public IReadOnlyList<IExpComponent> Args { get; private set; }

        public NewExp(string type, IEnumerable<string> typeArgs, IEnumerable<IExpComponent> args)
        {
            this.Type = type;
            this.TypeArgs = typeArgs.ToList();
            this.Args = args.ToList();
        }

        public void Visit(IExpComponentVisitor visitor)
        {
            visitor.Visit(this);
        }
    }

    public class StringExp : IExpComponent
    {
        public string Value { get; private set; }

        public StringExp(string value)
        {
            this.Value = value;
        }

        public void Visit(IExpComponentVisitor visitor)
        {
            visitor.Visit(this);
        }
    }

    public class UnaryExp : IExpComponent
    {
        public UnaryExpKind Operation { get; private set; }
        public IExpComponent Operand { get; private set; }

        public UnaryExp(UnaryExpKind operation, IExpComponent operand)
        {
            this.Operation = operation;
            this.Operand = operand;
        }

        public void Visit(IExpComponentVisitor visitor)
        {
            visitor.Visit(this);
        }
    }

    public class VariableExp : IExpComponent
    {
        public string VarName { get; private set; }

        public VariableExp(string varName)
        {
            this.VarName = varName;
        }

        public void Visit(IExpComponentVisitor visitor)
        {
            visitor.Visit(this);
        }
    }

    public class FileUnit
    {
        public IReadOnlyList<IFileUnitComponent> Comps { get; private set; }

        public FileUnit(IEnumerable<IFileUnitComponent> comps)
        {
            this.Comps = comps.ToList();
        }
    }

    public class NameAndExp
    {
        public string VarName { get; private set; }
        public IExpComponent Exp { get; private set; }

        public NameAndExp(string varName, IExpComponent exp)
        {
            this.VarName = varName;
            this.Exp = exp;
        }
    }

    public class FuncParam
    {
        public IReadOnlyList<FuncModifier> Modifiers { get; private set; }
        public string Type { get; private set; }
        public string VarName { get; private set; }

        public FuncParam(IEnumerable<FuncModifier> modifiers, string type, string varName)
        {
            this.Modifiers = modifiers.ToList();
            this.Type = type;
            this.VarName = varName;
        }
    }

}
