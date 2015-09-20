using Gum.Prerequisite.Generator;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gum.Prerequisite
{
    class GenerateAbstractSyntax
    {
        // FuncParam, Call일때만 Argument


        static Structure GenerateStucture()
        {
            var structure = new Structure();

            var stringType = structure.CreatePrimitive("string");
            var intType = structure.CreatePrimitive("int");
            var boolType = structure.CreatePrimitive("bool");
            var charType = structure.CreatePrimitive("char");

            // enumeration
            var unaryExpKind = structure.CreateEnum("UnaryExpKind");
            var binaryExpKind = structure.CreateEnum("BinaryExpKind");
            var funcParamModifier = structure.CreateEnum("FuncParamModifier");
            var memberFuncModifier = structure.CreateEnum("MemberFuncModifier");
            var accessModifier = structure.CreateEnum("AccessModifier");
            var virtualModifier = structure.CreateEnum("VirtualModifier");

            var stmtComponent = structure.CreateComponent("IStmtComponent");
            var expComponent = structure.CreateComponent("IExpComponent");
            var namespaceComponent = structure.CreateComponent("INamespaceComponent");
            var fileUnitComponent = structure.CreateComponent("IFileUnitComponent");
            var memberComponent = structure.CreateComponent("IMemberComponent");

            // directives
            var usingDirective = structure.CreateStruct("UsingDirective");

            // Decls
            var namespaceDecl = structure.CreateStruct("NamespaceDecl");
            var funcDecl = structure.CreateStruct("FuncDecl");
            var varDecl = structure.CreateStruct("VarDecl");
            var classDecl = structure.CreateStruct("ClassDecl");
            var structDecl = structure.CreateStruct("StructDecl");
            var memberFuncDecl = structure.CreateStruct("MemberFuncDecl");
            var memberVarDecl = structure.CreateStruct("MemberVarDecl");

            // Stmts
            var blockStmt = structure.CreateStruct("BlockStmt");
            var breakStmt = structure.CreateStruct("BreakStmt");
            var continueStmt = structure.CreateStruct("ContinueStmt");
            var doWhileStmt = structure.CreateStruct("DoWhileStmt");
            var expStmt = structure.CreateStruct("ExpStmt");
            var forStmt = structure.CreateStruct("ForStmt");
            var ifStmt = structure.CreateStruct("IfStmt");
            var returnStmt = structure.CreateStruct("ReturnStmt");
            var whileStmt = structure.CreateStruct("WhileStmt");

            // Exps
            var assignExp = structure.CreateStruct("AssignExp");
            var binaryExp = structure.CreateStruct("BinaryExp");
            var boolExp = structure.CreateStruct("BoolExp");
            var charExp = structure.CreateStruct("CharExp");
            var callExp = structure.CreateStruct("CallExp");     // <>(); expression
            var memberExp = structure.CreateStruct("MemberExp"); // a.b
            var integerExp = structure.CreateStruct("IntegerExp");
            var newExp = structure.CreateStruct("NewExp");
            var stringExp = structure.CreateStruct("StringExp");
            var unaryExp = structure.CreateStruct("UnaryExp");
            var idExp = structure.CreateStruct("IDExp");

            var fileUnit = structure.CreateStruct("FileUnit");

            var nameAndExp = structure.CreateStruct("NameAndExp");
            var funcParam = structure.CreateStruct("FuncParam");
            var idWithTypeArgs = structure.CreateStruct("IDWithTypeArgs");
            

            unaryExpKind
                .Add("Minus")
                .Add("Neg")
                .Add("Not")
                .Add("PrefixInc")
                .Add("PrefixDec")
                .Add("PostfixInc")
                .Add("PostfixDec");

            binaryExpKind
                .Add("Equal")
                .Add("NotEqual")
                .Add("And")
                .Add("Or")
                .Add("Add")
                .Add("Sub")
                .Add("Mul")
                .Add("Div")
                .Add("Mod")
                .Add("Less")
                .Add("Greater")
                .Add("LessEqual")
                .Add("GreaterEqual");

            funcParamModifier
                .Add("Out")
                .Add("Parameters");

            // 
            fileUnit
                .Vars(fileUnitComponent, "Comps");

            fileUnitComponent
                .Add(usingDirective)
                .Add(namespaceDecl)
                .Add(varDecl)
                .Add(funcDecl);

            usingDirective
                .Vars(stringType, "Names");

            namespaceDecl
                .Vars(stringType, "Names")
                .Vars(namespaceComponent, "Comps");

            namespaceComponent
                .Add(namespaceDecl)
                .Add(varDecl)
                .Add(funcDecl)
                .Add(classDecl)
                .Add(structDecl);

            // Name<Arg0, Arg1>
            idWithTypeArgs
                .Var(stringType, "Name")
                .Vars(idWithTypeArgs, "Args");

            // Type NameAndExps0.Name = NameAndExps0.Exp, NameAndExps1.Name = NameAndExps1.Exp;
            varDecl
                .Var(idWithTypeArgs, "Type")
                .Vars(nameAndExp, "NameAndExps");

            // VarName = Exp
            nameAndExp
                .Var(stringType, "VarName")
                .Var(expComponent, "Exp");

            // exps
            expComponent
                .Add(assignExp)
                .Add(binaryExp)
                .Add(boolExp)
                .Add(charExp)
                .Add(callExp)
                .Add(memberExp)
                .Add(integerExp)
                .Add(newExp)
                .Add(stringExp)
                .Add(unaryExp)
                .Add(idExp);

            // Left = Right
            assignExp
                .Var(expComponent, "Left")
                .Var(expComponent, "Right");

            // Operand1 Operation Operand2
            binaryExp
                .Var(binaryExpKind, "Operation")
                .Var(expComponent, "Operand1")
                .Var(expComponent, "Operand2");

            // Value 
            boolExp
                .Var(boolType, "Value");

            charExp
                .Var(charType, "Value");
            
            // FuncExp(Arg0, Arg1)
            callExp
                .Var(expComponent, "FuncExp")
                .Vars(expComponent, "Args");

            // Exp.(ID.Name)
            memberExp
                .Var(expComponent, "Exp")
                .Var(stringType, "MemberName");

            // Value(0, .. )
            integerExp
                .Var(intType, "Value");

            // new Type(Arg0, Arg1)
            newExp
                .Var(idWithTypeArgs, "TypeName")
                .Vars(expComponent, "Args");

            // Value("1")
            stringExp
                .Var(stringType, "Value");

            // Operation Operand
            unaryExp
                .Var(unaryExpKind, "Operation")
                .Var(expComponent, "Operand");

            // f<int>
            idExp
                .Var(idWithTypeArgs, "Name");

            // List<int> IFunc<String>.Func(int a) { }
            funcDecl
                .Vars(stringType, "TypeVars")
                .Var(idWithTypeArgs, "ReturnType")
                .Var(stringType, "Name") 
                .Vars(funcParam, "Parameters")
                .Var(blockStmt, "Body"); // TODO: BlockStmt?
           
            memberFuncModifier
                .Add("Static")
                .Add("New");

            accessModifier
                .Add("Public")
                .Add("Protected")
                .Add("Private");

            virtualModifier
                .Add("Virtual")
                .Add("Override");

            memberFuncDecl
                .Vars(memberFuncModifier, "FuncModifiers")
                .Var(accessModifier, "AccessModifier")
                .Var(virtualModifier, "VirtualModifier")
                
                .Vars(stringType, "TypeParams")                
                .Var(idWithTypeArgs, "ReturnType")
                .Var(idWithTypeArgs, "InterfaceType" )
                .Var(stringType, "Name")
                .Vars(funcParam, "Parameters")
                .Var(blockStmt, "Body");

            memberVarDecl
                .Vars(accessModifier, "AccessModifier")
                .Var(idWithTypeArgs, "Type")
                .Vars(stringType, "Names");

            memberComponent
                .Add(memberFuncDecl)
                .Add(memberVarDecl);

            // class Name<TypeVar0, TypeVar1> : BaseType0, BaseType1 { Components }
            classDecl
                .Vars(stringType, "TypeVars") // 선언에는 typeID가 필요없다                
                .Var(stringType, "Name")
                .Vars(idWithTypeArgs, "BaseTypes")
                .Vars(memberComponent, "Components");

            structDecl
                .Vars(stringType, "TypeVars")
                .Var(stringType, "Name")
                .Vars(idWithTypeArgs, "BaseTypes")
                .Vars(memberComponent, "Components");

            // stmtComponent
            stmtComponent
                .Add(varDecl)
                .Add(blockStmt)
                .Add(breakStmt)
                .Add(continueStmt)
                .Add(doWhileStmt)
                .Add(expStmt)
                .Add(forStmt)
                .Add(ifStmt)
                .Add(returnStmt)
                .Add(whileStmt);

            blockStmt
                .Vars(stmtComponent, "Stmts");

            // breakStmt
            // continueStmt 
            doWhileStmt
                .Var(stmtComponent, "Body")
                .Var(expComponent, "CondExp");

            expStmt
                .Var(expComponent, "Exp");

            forStmt
                .Var(stmtComponent, "Initializer")
                .Var(expComponent, "CondExp")
                .Var(expComponent, "LoopExp")
                .Var(stmtComponent, "Body");

            ifStmt
                .Var(expComponent, "CondExp")
                .Var(stmtComponent, "ThenStmt")
                .Var(stmtComponent, "ElseStmt");

            returnStmt
                .Var(expComponent, "ReturnExp");

            whileStmt
                .Var(expComponent, "CondExp")
                .Var(stmtComponent, "Body");

            funcParam
                .Vars(funcParamModifier, "Modifiers")
                .Var(idWithTypeArgs, "Type")
                .Var(stringType, "VarName");            

            return structure;
        }

        static public void GenerateCSharpFile()
        {
            var structure = GenerateStucture();

            // Src
            Directory.CreateDirectory(@"..\..\Src\Gum.Lang\AbstractSyntax");
            using(var streamWriter = new StreamWriter(@"..\..\Src\Gum.Lang\AbstractSyntax\Generated.cs"))
            {
                var printer = new CSharpPrinter("Gum.Lang.AbstractSyntax");
                structure.Print(printer, streamWriter);

                // var printer = new SimplePrinter();
                // structure.Print(printer, Console.Out);
            }
        }
    }
}
