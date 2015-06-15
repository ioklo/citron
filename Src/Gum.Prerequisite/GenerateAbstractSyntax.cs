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
        static Structure GenerateStucture()
        {
            var structure = new Structure();

            var stringType = structure.CreatePrimitive("string");
            var intType = structure.CreatePrimitive("int");
            var boolType = structure.CreatePrimitive("bool");

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
            var continueStmt = structure.CreateStruct("ContineStmt");
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
            var callExp = structure.CreateStruct("CallExp");
            var fieldExp = structure.CreateStruct("FieldExp");
            var integerExp = structure.CreateStruct("IntegerExp");
            var newExp = structure.CreateStruct("NewExp");
            var stringExp = structure.CreateStruct("StringExp");
            var unaryExp = structure.CreateStruct("UnaryExp");
            var variableExp = structure.CreateStruct("VariableExp");

            var fileUnit = structure.CreateStruct("FileUnit");

            var nameAndExp = structure.CreateStruct("NameAndExp");
            var funcParam = structure.CreateStruct("FuncParam");

            unaryExpKind
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
                .Add("Params");

            // 
            fileUnit
                .Vars(fileUnitComponent, "Comps");

            fileUnitComponent
                .Add(usingDirective)
                .Add(namespaceDecl)
                .Add(varDecl)
                .Add(funcDecl);

            usingDirective
                .Var(stringType, "NamespaceName");

            namespaceDecl
                .Var(stringType, "VarName")
                .Vars(namespaceComponent, "Comps");

            namespaceComponent
                .Add(varDecl)
                .Add(funcDecl)
                .Add(classDecl)
                .Add(structDecl);

            varDecl
                .Var(stringType, "Type")
                .Vars(nameAndExp, "NameAndExps");

            nameAndExp
                .Var(stringType, "VarName")
                .Var(expComponent, "Exp");

            // exps
            expComponent
                .Add(assignExp)
                .Add(binaryExp)
                .Add(boolExp)
                .Add(callExp)
                .Add(fieldExp)
                .Add(integerExp)
                .Add(newExp)
                .Add(stringExp)
                .Add(unaryExp)
                .Add(variableExp);

            assignExp
                .Var(expComponent, "Left")
                .Var(expComponent, "Right");

            binaryExp
                .Var(binaryExpKind, "Operation")
                .Var(expComponent, "Operand1")
                .Var(expComponent, "Operand2");

            boolExp
                .Var(boolType, "Value");

            callExp
                .Var(expComponent, "FuncExp")
                .Vars(expComponent, "Args");

            fieldExp
                .Var(expComponent, "Exp")
                .Var(stringType, "ID");

            integerExp
                .Var(intType, "Value");

            newExp
                .Var(stringType, "Type")
                .Vars(stringType, "TypeArgs")
                .Vars(expComponent, "Args");

            stringExp
                .Var(stringType, "Value");

            unaryExp
                .Var(unaryExpKind, "Operation")
                .Var(expComponent, "Operand");

            variableExp
                .Var(stringType, "Name");

            funcDecl
                .Var(stringType, "ReturnType")
                .Var(stringType, "Name")
                .Vars(funcParam, "FuncParams")
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
                .Var(stringType, "ReturnType")
                .Var(stringType, "Name")
                .Vars(funcParam, "FuncParams")
                .Var(blockStmt, "Block");

            memberVarDecl
                .Vars(accessModifier, "AccessModifier")
                .Var(stringType, "Type")
                .Var(stringType, "Name");

            memberComponent
                .Add(memberFuncDecl)
                .Add(memberVarDecl);

            classDecl
                .Var(stringType, "Name")
                .Vars(stringType, "BaseTypes")
                .Vars(memberComponent, "Components");

            structDecl
                .Var(stringType, "Name")
                .Vars(stringType, "BaseTypes")
                .Vars(memberComponent, "Components");

            // stmtComponent
            stmtComponent
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
                .Var(stringType, "Type")
                .Var(stringType, "VarName");

            return structure;
        }

        static public void GenerateCSharpFile()
        {
            var structure = GenerateStucture();

            // Src
            using(var streamWriter = new StreamWriter(@"..\..\Src\Gum.Lang\AbstractSyntax\Generated.cs"))
            {
                var printer = new CSharpPrinter("Gum.Lang.AbstractSyntax");

                
                structure.Print(printer, streamWriter);
            }
        }
    }
}
