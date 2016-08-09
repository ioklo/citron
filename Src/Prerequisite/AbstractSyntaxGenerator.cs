using Gum.Prerequisite.Generator;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gum.Prerequisite
{
    class AbstractSyntaxGenerator
    {
        // FuncParam, Call일때만 Argument
        public static Structure Generate()
        {
            var structure = new Structure();

            var stringType = structure.CreatePrimitive("string");
            var intType = structure.CreatePrimitive("int");
            var boolType = structure.CreatePrimitive("bool");
            var typeType = structure.CreatePrimitive("IType");
            
            // enumeration
            var unaryExpKind = structure.CreateEnum("UnaryExpKind");
            var binaryExpKind = structure.CreateEnum("BinaryExpKind");
            var funcParamModifier = structure.CreateEnum("FuncParamModifier");
            var memberFuncModifier = structure.CreateEnum("MemberFuncModifier");
            var memberVarModifier = structure.CreateEnum("MemberVarModifier");
            
            var stmtComponent = structure.CreateComponent("IStmtComponent");
            var expComponent = structure.CreateComponent("IExpComponent");
            var namespaceComponent = structure.CreateComponent("INamespaceComponent");
            var fileUnitComponent = structure.CreateComponent("IFileUnitComponent");
            var memberComponent = structure.CreateComponent("IMemberComponent");
            var forInitComponent = structure.CreateComponent("IForInitComponent");

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
            var varDeclStmt = structure.CreateStruct("VarDeclStmt");
            var blankStmt = structure.CreateStruct("BlankStmt");
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
            var callExp = structure.CreateStruct("CallExp");     // <>(); expression
            var memberExp = structure.CreateStruct("MemberExp"); // a.b
            var integerExp = structure.CreateStruct("IntegerExp");
            var newExp = structure.CreateStruct("NewExp");
            var stringExp = structure.CreateStruct("StringExp");
            var unaryExp = structure.CreateStruct("UnaryExp");
            var idExp = structure.CreateStruct("IDExp");
            var arrayExp = structure.CreateStruct("ArrayExp");

            var fileUnit = structure.CreateStruct("FileUnit");

            var nameAndExp = structure.CreateStruct("NameAndExp");
            var funcParam = structure.CreateStruct("FuncParam");
            var namespaceID = structure.CreateStruct("NamespaceID");
            var typeID = structure.CreateStruct("TypeID");
            var idWithTypeArgs = structure.CreateStruct("IDWithTypeArgs");
            

            unaryExpKind
                .Add("Plus")
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
                .Add("ConditionalAnd")
                .Add("ConditionalOr")
                .Add("LogicalAnd")
                .Add("LogicalXor")
                .Add("LogicalOr")
                .Add("Add")
                .Add("Sub")
                .Add("Mul")
                .Add("Div")
                .Add("Mod")
                .Add("Less")
                .Add("Greater")
                .Add("LessEqual")
                .Add("GreaterEqual")
                .Add("Assign")
                .Add("ShiftLeft")
                .Add("ShiftRight");

            funcParamModifier
                .Add("Out")
                .Add("Parameters");

            // 
            fileUnit
                .Vars(fileUnitComponent, "Comps");

            fileUnitComponent
                // .Add(usingDirective)
                // .Add(namespaceDecl)
                // .Add(classDecl);
                // .Add(funcDecl)
                .Add(stmtComponent);

            usingDirective
                .Var(namespaceID, "NamespaceID");

            namespaceDecl
                .Var(namespaceID, "NamespaceID")
                .Vars(namespaceComponent, "Comps");

            namespaceComponent
                .Add(namespaceDecl)
                // .Add(varDecl)
                // .Add(funcDecl)
                .Add(classDecl)
                .Add(structDecl);

            // Name<Arg0, Arg1>
            idWithTypeArgs
                .Var(stringType, "Name")
                .Vars(typeID, "Args");

            // namespace1.namespace2
            namespaceID
                .Vars(stringType, "Names");

            // namespace1.namespace2.TypeName0.TypeName1<Args0, Args1>
            typeID
                .Vars(idWithTypeArgs, "IDs");

            // Type NameAndExps0.Name = NameAndExps0.Exp, NameAndExps1.Name = NameAndExps1.Exp;
            varDecl
                 .Var(typeID, "Type")
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
                .Add(callExp)
                .Add(memberExp)
                .Add(integerExp)
                .Add(newExp)
                .Add(stringExp)
                .Add(unaryExp)
                .Add(idExp)
                .Add(arrayExp);

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

            // FuncExp(Arg0, Arg1)
            callExp 
                .Var(expComponent, "FuncExp")
                .Vars(expComponent, "Args");

            // Exp.(ID.Name)
            memberExp
                .Var(expComponent, "Exp")
                .Var(stringType, "MemberName")
                .Vars(typeID, "TypeArgs");

            // Value(0, .. )
            integerExp
                .Var(intType, "Value");

            // new Type(Arg0, Arg1)
            newExp
                .Var(typeID, "TypeName")
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
                .Var(stringType, "ID")
                .Vars(typeID, "typeArgs");

            arrayExp
                .Var(expComponent, "Exp")
                .Var(expComponent, "Index");
            
            // List<int> IFunc<String>.Func<int>(int a) { }
            //funcDecl
            //    .Var(typeID, "ReturnType")
            //    .Var(stringType, "Name")
            //    .Vars(stringType, "TypeVars")
            //    .Vars(funcParam, "Parameters")
            //    .Var(blockStmt, "Body"); // TODO: BlockStmt?
           
            memberFuncModifier
                .Add("Static")
                .Add("New")
                .Add("Public")
                .Add("Protected")
                .Add("Private")
                .Add("Virtual")
                .Add("Override");

            memberFuncDecl
                .Vars(memberFuncModifier, "Modifiers")
                .Vars(stringType, "TypeParams")                
                .Var(typeID, "ReturnType")                
                .Var(stringType, "Name")
                .Vars(funcParam, "Parameters")
                .Vars(stmtComponent, "BodyStmts");

            memberVarDecl
                .Vars(memberVarModifier, "Modifiers")
                .Var(typeID, "Type")
                .Vars(stringType, "Names");

            memberComponent
                .Add(memberFuncDecl)
                .Add(memberVarDecl);

            forInitComponent
                .Add(varDecl)
                .Add(expComponent);

            // class Name<TypeVar0, TypeVar1> : BaseType0, BaseType1 { Components }
            classDecl
                .Vars(stringType, "TypeVars") // 선언에는 typeID가 필요없다                
                .Var(stringType, "Name")
                .Vars(typeID, "BaseTypes")
                .Vars(memberComponent, "Components");

            structDecl
                .Vars(stringType, "TypeVars")
                .Var(stringType, "Name")
                .Vars(typeID, "BaseTypes")
                .Vars(memberComponent, "Components");

            // stmtComponent
            stmtComponent
                .Add(varDeclStmt)
                .Add(blankStmt)
                .Add(blockStmt)
                .Add(breakStmt)
                .Add(continueStmt)
                .Add(doWhileStmt)
                .Add(expStmt)
                .Add(forStmt)
                .Add(ifStmt)
                .Add(returnStmt)
                .Add(whileStmt);

            varDeclStmt
                .Var(typeID, "Type")
                .Vars(nameAndExp, "NameAndExps");

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
                .Var(forInitComponent, "Initializer")
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
                .Var(typeID, "Type")
                .Var(stringType, "VarName");            

            return structure;
        }
    }
}
