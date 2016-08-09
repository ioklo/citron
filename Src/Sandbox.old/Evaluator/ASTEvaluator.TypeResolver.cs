using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gum.Lang.AbstractSyntax;
using System.Reflection;

namespace Gum.Evaluator
{
    partial class ASTEvaluator
    {
        // 글로벌하게 타입을 가져올 수 있는 방식
        interface IReference
        {
            // TypeID를 가지고
            bool TryGetType(out IType type, IReadOnlyList<string> id, int typeParamCount);
        }

        interface IPrimitiveReference
        {
            IType BoolType { get; }
            IType ByteType { get; }
            IType IntType { get; }
            IType StringType { get; }
        }

        class DotnetValue : Value
        {

        }

        class DotnetType : IType
        {
            Type type;

            public DotnetType(Type type)
            {
                this.type = type;
            }

            // Drived만 판단합니다.. 
            public bool CanConvertTo(IType targetType)
            {
                DotnetType targetDotnetType = targetType as DotnetType;
                if (targetDotnetType == null) return false;

                return targetDotnetType.type.IsAssignableFrom(type);
            }

            public bool TryGetMemberType(out IType type, string memberName)
            {
                type = null;

                var memberInfos = this.type.GetMember(memberName);
                if (memberInfos.Length != 1)
                    return false;

                if (memberInfos[0].MemberType == MemberTypes.Field)
                    type = new DotnetType(((FieldInfo)memberInfos[0]).FieldType);

                else if (memberInfos[0].MemberType == MemberTypes.Property)
                    type = new DotnetType(((PropertyInfo)memberInfos[0]).PropertyType);

                return type != null;
            }

            // func<T, U> constraint를 어떻게 할 것인지
            public bool TryGetMemberType(out IType type, string memberName, IReadOnlyList<IType> typeArgs, IReadOnlyList<IType> argTypes)
            {
                var memberInfos = this.type.GetMember(memberName);

                foreach(var memberInfo in memberInfos)
                {
                    foreach( ParameterInfo pi in ((MethodInfo)methodInfo).GetParameters())
                    {
                    }
                }

                // nested class에 대해서 어떻게 처리할 것인가

                throw new NotImplementedException();
            }
        }

        class DotnetReference : IReference
        {
            Assembly assembly;

            public bool TryGetType(out IType type, IReadOnlyList<string> ids, int typeParamCount)
            {
                string csharpName;

                if (typeParamCount == 0)
                    csharpName = string.Join(".", ids);
                else
                    csharpName = string.Format("{0}`{1}", string.Join(".", ids), typeParamCount);

                var csharpType = assembly.GetType(csharpName);
                if (csharpType == null)
                {
                    type = null;
                    return false;
                }

                type = new DotnetType(csharpType);
                return true;
            }
        }


        // 가장 기본적인 PrimitiveReference
        // gum의 int는 CSharp의 int타입
        class CSharpPrimitiveReference : IPrimitiveReference
        {
            // TryGetType "System.Int32"
            public bool TryGetType(out IType type, IReadOnlyList<string> id, int typeParamCount)
            {

            }
        }

        class ASTTypeResolver : 
            IFileUnitComponentVisitor, 
            IStmtComponentVisitor, 
            IExpComponentVisitor, 
            IForInitComponentVisitor
        {
            // type을 Resolve하는데 필요한 정보들

            List<IReference> references;

            // TypeResolver.AddReference(new CSharpReference())
            // TypeResolver.AddReference(new GumPrimitiveReference());

            public void Resolve(FileUnit fileUnit)
            {
                foreach(var comp in fileUnit.Comps)
                    comp.Visit(this);
            }

            public void Resolve(IStmtComponent stmt)
            {
                stmt.Visit(this);
            }

            public void Resolve(IExpComponent exp)
            {
                exp.Visit(this);
            }

            public void Resolve(IForInitComponent forInitComp)
            {
                forInitComp.Visit(this);
            }

            public void Resolve(TypeID typeID)
            {
                // 현재 context가 없고, using namespace도 사용하지 않으므로.. 
                // 통으로 검사합니다

                // namespace가 있다면.. 하나씩 붙여보고...여러개 있으면 모호성이 있다고 판단하면 됩니다.

                // 지금은 그냥 모든 레퍼런스에서 검사

                var types = new List<IType>();
                foreach(var reference in references)
                {
                    IType type = reference.FindType();
                    types.Add(type);
                }
                
            }

            public void Visit(ContinueStmt continueStmt)
            {
            }

            public void Visit(ExpStmt expStmt)
            {
                Resolve(expStmt.Exp);                
            }

            public void Visit(IfStmt ifStmt)
            {
                Resolve(ifStmt.CondExp);
                Resolve(ifStmt.ThenStmt);
                Resolve(ifStmt.ElseStmt);
            }

            public void Visit(BinaryExp binaryExp)
            {
                Resolve(binaryExp.Operand1);
                Resolve(binaryExp.Operand2);
            }

            public void Visit(CallExp callExp)
            {
                Resolve(callExp.FuncExp);
                foreach (var arg in callExp.Args)
                    Resolve(arg);
            }

            public void Visit(IntegerExp integerExp)
            {
            }

            public void Visit(StringExp stringExp)
            {
            }

            public void Visit(IDExp idexp)
            {
                foreach(var arg in idexp.typeArgs)
                    Resolve(arg);
            }

            public void Visit(IExpComponent exp)
            {
                Resolve(exp);
            }

            public void Visit(VarDecl varDecl)
            {
                Resolve(varDecl.Type);
                foreach (var nameAndExp in varDecl.NameAndExps)
                    Resolve(nameAndExp.Exp);
            }

            public void Visit(ArrayExp arrayExp)
            {
                Resolve(arrayExp.Exp);
                Resolve(arrayExp.Index);
            }

            public void Visit(UnaryExp unaryExp)
            {
                Resolve(unaryExp.Operand);
            }

            public void Visit(NewExp newExp)
            {
                Resolve(newExp.TypeName);
                foreach (var arg in newExp.Args)
                    Resolve(arg);
            }

            public void Visit(MemberExp memberExp)
            {
                Resolve(memberExp.Exp);
                foreach(var typeArg in memberExp.TypeArgs)
                    Resolve(typeArg);
            }

            public void Visit(BoolExp boolExp)
            {
            }

            public void Visit(AssignExp assignExp)
            {
                Resolve(assignExp.Left);
                Resolve(assignExp.Right);
            }

            public void Visit(WhileStmt whileStmt)
            {
                Resolve(whileStmt.CondExp);
                Resolve(whileStmt.Body);
            }

            public void Visit(ReturnStmt returnStmt)
            {
                if(returnStmt.ReturnExp != null)
                    Resolve(returnStmt.ReturnExp);
            }

            public void Visit(ForStmt forStmt)
            {
                Resolve(forStmt.Initializer);
                Resolve(forStmt.CondExp);
                Resolve(forStmt.Body);
                Resolve(forStmt.LoopExp);
            }

            public void Visit(DoWhileStmt doWhileStmt)
            {
                Resolve(doWhileStmt.CondExp);
                Resolve(doWhileStmt.Body);
            }

            public void Visit(BreakStmt breakStmt)
            {
            }

            public void Visit(BlankStmt blankStmt)
            {
            }

            public void Visit(BlockStmt blockStmt)
            {
                foreach (var stmt in blockStmt.Stmts)
                    Resolve(stmt);
            }

            public void Visit(VarDeclStmt varDeclStmt)
            {
                Resolve(varDeclStmt.Type);
                foreach (var nameAndExp in varDeclStmt.NameAndExps)
                    Resolve(nameAndExp.Exp);
            }

            public void Visit(IStmtComponent stmt)
            {
                Resolve(stmt);
            }


        }
    }
}
