using Gum.Lang.AbstractSyntax;
using Gum.Test;
using Gum.Test.Metadata;
using Gum.Test.Type;
using System;
using System.Collections.Generic;
using System.Linq;
using Environment = Gum.Test.Environment;

namespace Gum.AST
{
    class TypeAnnotator : 
        IFileUnitComponentVisitor,
        INamespaceComponentVisitor,
        IStmtComponentVisitor,
        IExpComponentVisitor<IType>
    {
        Dictionary<IExpComponent, IType> expTypes = new Dictionary<IExpComponent, IType>();
        StringMap<IType> varTypes = new StringMap<IType>();
        Stack<IType> retTypes = new Stack<IType>();

        Environment env;

        public TypeAnnotator(GumMetadata metadata, IEnumerable<IMetadata> externMetadata)
        {
            env = new Environment(metadata, externMetadata);
        }

        public void Visit(FileUnit fileUnit)
        {
            foreach (var comp in fileUnit.Comps)
                Visit(comp);
        }

        #region Visitor Entrance
        public void Visit(IFileUnitComponent fileUnit)
        {
            fileUnit.Visit(this);
        }

        public void Visit(INamespaceComponent namespaceComp)
        {
            namespaceComp.Visit(this);
        }

        public void Visit(IStmtComponent stmtComp)
        {
            stmtComp.Visit(this);
        }

        public IType Visit(IExpComponent exp)
        {
            return exp.Visit(this);
        }
        #endregion

        public void Visit(UsingDirective usingDirective)
        {
            // using directives
            env.AddUsing(usingDirective.Names);
        }

        public void Visit(NamespaceDecl namespaceDecl)
        {
            env.PushNamespace(namespaceDecl.Names);

            foreach (var comp in namespaceDecl.Comps)
                comp.Visit(this);

            env.PopNamespace();
        }

        public void Visit(VarDecl varDecl)
        {
            var type = env.GetType(varDecl.Type);

            foreach (var nameAndExp in varDecl.NameAndExps)
            {
                varTypes.Add(nameAndExp.VarName, type);

                if (nameAndExp.Exp != null)
                    Visit(nameAndExp.Exp);
            }
        }

        public void Visit(FuncDecl funcDecl)
        {
            // Type Argument 넣기
            env.PushFuncTypeVars(funcDecl.TypeVars);

            varTypes.Push();

            foreach (var param in funcDecl.Parameters)
            {
                var paramType = env.GetType(param.Type);
                varTypes.Add(param.VarName, paramType);
            }

            // 리턴 타입 비교를 위해 넣어줍니다
            var retType = env.GetType(funcDecl.ReturnType);
            retTypes.Push(retType);

            Visit(funcDecl.Body);

            varTypes.Pop();
            env.PopTypeArgs();
        }

        public void Visit(BlockStmt blockStmt)
        {
            foreach (var stmt in blockStmt.Stmts)
            {
                varTypes.Push();
                stmt.Visit(this);
                varTypes.Pop();
            }
        }

        public void Visit(BreakStmt breakStmt)
        {
        }

        public void Visit(ContineStmt contineStmt)
        {
        }

        public void Visit(DoWhileStmt doWhileStmt)
        {
            varTypes.Push();

            Visit(doWhileStmt.Body);
            CheckBool(doWhileStmt.CondExp);

            varTypes.Pop();
        }

        public void Visit(ExpStmt expStmt)
        {
            Visit(expStmt.Exp);
        }

        public void Visit(ForStmt forStmt)
        {
            varTypes.Push();

            Visit(forStmt.Initializer);

            CheckBool(forStmt.CondExp);

            Visit(forStmt.LoopExp);

            Visit(forStmt.Body);
        }

        private void CheckBool(IExpComponent expComponent)
        {
            throw new NotImplementedException();
        }

        public void Visit(IfStmt ifStmt)
        {
            varTypes.Push();
            CheckBool(ifStmt.CondExp);
            Visit(ifStmt.ThenStmt);
            Visit(ifStmt.ElseStmt);
            varTypes.Pop();
        }

        public void Visit(ReturnStmt returnStmt)
        {
            var returnType = Visit(returnStmt.ReturnExp);

            Check(returnType, retTypes.Peek());
        }

        public void Visit(WhileStmt whileStmt)
        {
            varTypes.Push();
            CheckBool(whileStmt.CondExp);
            Visit(whileStmt.Body);
            varTypes.Pop();
        }

        public void Check(IType left, IType right)
        {
            throw new Exception();
        }

        #region IExpComponentVisitor<IType>
        public IType Visit(AssignExp assignExp)
        {
            var leftType = Visit(assignExp.Left);
            var rightType = Visit(assignExp.Right);

            // TypeCheck
            Check(leftType, rightType);
            expTypes.Add(assignExp, leftType);

            return leftType;
        }

        public IType Visit(BinaryExp binaryExp)
        {
            var type1 = Visit(binaryExp.Operand1);
            var type2 = Visit(binaryExp.Operand2);

            // 가능한 binary operation들을 검색해서 returnType을 알아와야 합니다
            FuncDef func = env.GetOperatorFunc(type1, type2, binaryExp.Operation);
            expTypes.Add(binaryExp, func.ReturnType);

            return func.ReturnType;
        }

        public IType Visit(BoolExp boolExp)
        {
            var boolType = env.GetBoolType();
            expTypes.Add(boolExp, boolType);
            return boolType;
        }

        public IType Visit(CharExp charExp)
        {
            // var charType = env.GetCharType();
            throw new NotImplementedException();

        }

        // (FuncExp)<TypeArgs>(Args)
        public IType Visit(CallExp callExp)
        {
            // 1.FuncExp를 사용해서 functionType을 가져옵니다. 여러 후보가 있을 수 있습니다
            IEnumerable<IType> argTypes = callExp.Args.Select(Visit);

            return GetFunctionTypes(callExp.FuncExp, argTypes);
        }

        // TODO: funcExp를 해석해서 typeArgs와 argTypes에 맞는 함수를 선택합니다
        private IType GetFunctionTypes(IExpComponent funcExp, IEnumerable<IType> argTypes)
        {
            throw new NotImplementedException();
        }

        // (Exp).(ID)
        public IType Visit(MemberExp memberExp)
        {
            IType expType = Visit(memberExp.Exp);

            expType.GetMemberTypes(env, memberExp.MemberName);
            throw new NotImplementedException();
        }

        public IType Visit(IntegerExp integerExp)
        {
            return env.GetIntType();
        }

        public IType Visit(NewExp newExp)
        {
            return env.GetType(newExp.TypeName);
        }

        public IType Visit(StringExp stringExp)
        {
            return env.GetStringType();
        }

        public IType Visit(UnaryExp unaryExp)
        {
            var operandType = Visit(unaryExp.Operand);

            // 가능한 binary operation들을 검색해서 returnType을 알아와야 합니다
            FuncDef func = env.GetOperatorFunc(operandType, unaryExp.Operation);
            return func.ReturnType;
        }

        // ID는 클래스, 구조체, 변수, 함수가 될 수 있다
        public IType Visit(IDExp idExp)
        {
            // A라는 클래스의 타입은 

            // Class_A : Class
            //   - A의 static 변수/함수                

            // A의 타입은 Class에서 상속받은 Class_A
            // B의 타입은 Class_B 이고, Class_A와는 상관없다

            // Namespace는 제거 하고 갑시다
            // Class, Struct, Function, Variable 모두 Namespace로부터 

            // 1. Class static variable을 참조하기 위한 용도
            // 3. Variable
            // 4. Function
            throw new NotImplementedException();
        }
        #endregion


        public void Visit(ClassDecl classDecl)
        {
        }

        public void Visit(StructDecl structDecl)
        {
        }        

        public static IDictionary<IExpComponent, IType> Annotate(FileUnit fileUnit, GumMetadata gumMetadata, IEnumerable<IMetadata> externalMetadatas )
        {
            var typeAnnotator = new TypeAnnotator(gumMetadata, externalMetadatas);
            typeAnnotator.Visit(fileUnit);
            return typeAnnotator.expTypes;
        }

    }
}
