using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gum.Data.AbstractSyntax;


namespace Gum.Evaluator
{

    partial class ASTEvaluator
    {
        class ScopedDictionary<Key, Value>
        {
            List<Dictionary<Key, Value>> stack = new List<Dictionary<Key, Value>>();

            public ScopedDictionary()
            {
                Push();
            }
            
            public void Push()
            {
                stack.Add(new Dictionary<Key, Value>());
            }

            public void Pop()
            {
                stack.RemoveAt(stack.Count - 1);
            }

            public void Add(Key key, Value value)
            {
                stack[stack.Count - 1].Add(key, value);
            }

            public bool TryGetValue(Key key, out Value value)
            {
                for (int t = stack.Count - 1; 0 <= t; t--)
                    if (stack[t].TryGetValue(key, out value))
                        return true;

                value = default(Value);
                return false;
            }
        }

        class TypeCheckEnv
        {
            private IPrimitiveReference primitiveReference = new CSharpPrimitiveReference();
            private List<IReference> references = new List<IReference>();

            public void AddReference(IReference reference)
            {
                references.Add(reference);
            }

            public ScopedDictionary<string, IType> IDTypeMap { get; private set; } = new ScopedDictionary<string, IType>(); 

            // 현재 타입을 기록해 둡니다.. Nested type은 알아낼 수 있으므로 기록할 필요가 없습니다
            public IType ThisType { get; private set; }
            public IFunction ThisFunc { get; private set; }

            // 편의상 두는 Utility
            public IType BoolType { get; }

            private IEnumerable<TypeID> GetCandidateTypeIDs(TypeID typeID)
            {
                // TODO: 현재 네임스페이스를 지원하지 않으므로 그냥 typeID를 리턴합니다
                yield return typeID;
            }

            private bool TryResolvePrimitiveType(out IType type, TypeID typeID)
            {
                if (typeID.IDs.Count != 1 )
                {
                    type = null;
                    return false;
                }

                var typeName = typeID.IDs[0];
                if (typeName.Name == "int" && typeName.Args.Count == 0)
                {
                    type = primitiveReference.IntType;
                    return true;
                }

                if( typeName.Name == "bool" && typeName.Args.Count == 0)
                {
                    type = primitiveReference.BoolType;
                    return true;
                }

                if (typeName.Name == "string" && typeName.Args.Count == 0)
                {
                    type = primitiveReference.StringType;
                    return true;
                }

                type = null;
                return false;
            }

            public IType ResolveType(TypeID typeID)
            {
                // 타입은 검색 순서.. 가 없습니다.. 여러개 나오면 무조건 에러처리 합니다

                // 1) 네임스페이스 prefix를 안붙인거
                // 2) 현재 네임스페이스 prefix를 붙인거
                // 3) using에 포함된 모든 네임스페이스 prefix

                // 를 현재 포함된 Reference에서 검색합니다

                List<IType> candidateTypes = new List<IType>();

                // primitive type
                IType candidateType;
                if (TryResolvePrimitiveType(out candidateType, typeID))
                    candidateTypes.Add(candidateType);

                foreach ( var candidateTypeID in GetCandidateTypeIDs(typeID) )
                    foreach( var reference in references )
                    {
                        if (!reference.TryGetType(out candidateType, candidateTypeID))
                            continue;

                        candidateTypes.Add(candidateType);
                    }

                if (candidateTypes.Count == 0 )
                    throw new TypeCheckResolveTypeIDFailed();

                if (2 <= candidateTypes.Count)
                    throw new TypeCheckResolveTypeIDTooManyCandidates();

                return candidateTypes[0];
            }

            // Variable이라고 부르면 안되는데..(함수 이름 포함) 여튼 지금 env로부터 variable로 참조가능한 타입을 모두 가져온다 
            // !! type은 검색하지 않습니다.. type은 오직 typeArgs로 넘어왔을 때만 resolve 가능하게 설계합니다
            // 1) 현재 scope variable
            // 2) this의 member variable/function
            // 3) class와 각 outer class의 static variable/function
            // 4) 가능한 모든 네임스페이스의 variable/function (겹치면 에러)
            public bool TryGetIDType(out IType type, string id, IReadOnlyList<IType> typeArgs)
            {
                // 1) 현재 scope variable
                if (typeArgs.Count != 0)
                    if (IDTypeMap.TryGetValue(id, out type))
                        return true;

                // 2), 3) this의 member
                if (ThisType.TryGetMemberType(out type, id, typeArgs))
                {
                    // TODO: Access Modifier 검사
                    return true;
                }

                // 4) 
                // TODO: namespace에 대해서 타입을 검사합니다

                // 
                type = null;
                return false;
            }

            internal IFunction GetConstructor(IType type, List<IReadOnlyList<IType>> argTypeCandidatesList)
            {
                throw new NotImplementedException();
            }
        }

        class TypeCheckNotFunc : Exception
        {

        }

        class TypeCheckFuncArgCountMismatch : Exception
        {

        }

        class TypeCheckFuncArgMismatch : Exception
        {

        } 

        class TypeCheckResolveTypeIDFailed : Exception
        {

        }

        class TypeCheckResolveTypeIDTooManyCandidates : Exception
        {

        }

        class TypeCheckResolveIDFailed : Exception
        {

        }

        class TypeCheckResolveMemberFailed : Exception
        {

        }

        class TypeCheckResolveFuncFailed : Exception
        {

        }

        class TypeCheckConversionFailed : Exception
        {
            public TypeCheckConversionFailed(IType from, IType to)
            {

            }
        }


        // Exp 타입체커는 어떤 Exp 에 대해서 가능한 타입들의 리스트를 돌려줍니다. 
        class ExpTypeChecker : IExpComponentVisitorRet<IReadOnlyList<IType>, TypeCheckEnv>
        {
            private static ExpTypeChecker check = new ExpTypeChecker();
            public static IReadOnlyList<IType> Check(IExpComponent exp, TypeCheckEnv env)
            {
                return exp.VisitRet(check, env);
            }

            private IReadOnlyList<IType> ResolveSimpleType(TypeCheckEnv env, string id)
            {
                var emptyTypeIDs = new List<TypeID>();
                var idWithTypeArgsList = new List<IDWithTypeArgs>();
                idWithTypeArgsList.Add(new IDWithTypeArgs(id, emptyTypeIDs));

                return CreateCandidates(env.ResolveType(new TypeID(idWithTypeArgsList)));
            }

            public IReadOnlyList<IType> VisitRet(IntegerExp integerExp, TypeCheckEnv env)
            {
                return ResolveSimpleType(env, "int");
            }

            public IReadOnlyList<IType> VisitRet(StringExp stringExp, TypeCheckEnv env)
            {
                return ResolveSimpleType(env, "string");
            }           

            private IReadOnlyList<IType> ResolveTypeArgs(TypeCheckEnv env, IReadOnlyList<TypeID> typeIDs)
            {
                return typeIDs.Select(env.ResolveType).ToList();
            } 

            public IReadOnlyList<IType> VisitRet(IDExp idexp, TypeCheckEnv env)
            {
                IType type;
                var typeArgs = ResolveTypeArgs(env, idexp.typeArgs);

                if (!env.TryGetIDType(out type, idexp.ID, typeArgs))
                    throw new TypeCheckResolveIDFailed();

                return CreateCandidates(type);
            }

            public IReadOnlyList<IType> VisitRet(ArrayExp arrayExp, TypeCheckEnv env)
            {
                throw new NotImplementedException();
            }

            public IReadOnlyList<IType> VisitRet(NewExp newExp, TypeCheckEnv env)
            {
                IType type = env.ResolveType(newExp.TypeName);

                // new의 인자들의 타입을 얻어냅니다. 인자의 가능한 타입이 한개 이상이 될 경우, 
                // 모든 Candidate를 보존하다가 GetConstructor에서 확정짓습니다..
                // new SomeClass(A.Func, i); // A.Func가 overload 되어있을 경우(인자가 (int, int) 일수도 있고 short 일수도 있고)
                var argTypeCandidatesList = newExp.Args.Select(arg => Check(arg, env)).ToList();

                // arg별로 가능한 모든 타입들을 모아서 Constructor를 얻어낼때 사용합니다
                // 지금 GetConstructor, UnaryOperator, BinaryOperator
                // FuncKind = Constructor | UnaryOperator | BinaryOperator | Normal
                IFunction func = env.GetConstructor(type, argTypeCandidatesList);
                if (func == null)
                    throw new TypeCheckResolveFuncFailed();
                
                return CreateCandidates( type );
            }

            private IReadOnlyList<IType> CreateCandidates(params IType[] types)
            {
                return types.ToList();
            }

            public IReadOnlyList<IType> VisitRet(UnaryExp unaryExp, TypeCheckEnv env)
            {
                var operandTypeCandidates = Check(unaryExp.Operand, env);

                IFunction func = env.GetUnaryOperator(unaryExp.Operation, operandTypeCandidates);
                if (func == null)
                    throw new TypeCheckResolveFuncFailed();

                return new List<IType> { func.ReturnType };
            }

            public IReadOnlyList<IType> VisitRet(BinaryExp binaryExp, TypeCheckEnv env)
            {
                var operand1TypeCandidates = Check(binaryExp.Operand1, env);
                var operand2TypeCandidates = Check(binaryExp.Operand2, env);

                IFunction func = env.GetBinaryOperator(binaryExp.Operation, operand1TypeCandidates, operand2TypeCandidates);
                if (func == null)
                    throw new TypeCheckResolveFuncFailed();

                return CreateCandidates(func.ReturnType);
            }

            public IReadOnlyList<IType> VisitRet(CallExp callExp, TypeCheckEnv env)
            {
                // f = a.b.c(1, 2, 3)
                // (exp)(1, 2, 3);

                var funcTypeCandidates = Check(callExp.FuncExp, env);
                var argTypeCandidatesList = callExp.Args.Select(arg => Check(arg, env)).ToList();

                // funcExp에서 가능한 모든 후보와 인자를 매칭시킨다
                var funcType = ChooseFuncType(funcTypeCandidates, argTypeCandidatesList) as FuncType;
                return CreateCandidates(funcType.ReturnType);
            }

            // exp.id 의 가능한 타입 열거하기
            public IReadOnlyList<IType> VisitRet(MemberExp memberExp, TypeCheckEnv env)
            {
                IType memberExpType = Check(memberExp.Exp, env).Single();
                var argTypes = ResolveTypeArgs(env, memberExp.TypeArgs);
                
                IType memberType;
                if (!memberExpType.TryGetMemberType(out memberType, memberExp.MemberName, argTypes ))
                     throw new TypeCheckResolveMemberFailed();
                // return CreateCandidates(memberType);
            }

            public IReadOnlyList<IType> VisitRet(BoolExp boolExp, TypeCheckEnv env)
            {
                return ResolveSimpleType(env, "bool");
            }

            public IReadOnlyList<IType> VisitRet(AssignExp assignExp, TypeCheckEnv env)
            {
                // assignment는 왼쪽은 무조건 한 타입으로 Resolve 되어야 하며
                // 오른쪽은 왼쪽 식의 타입에 맞게 변환 되는지를 검사합니다

                var leftType = Check(assignExp.Left, env)
                    .Single();

                var rightType = Check(assignExp.Right, env)
                    .Where(typeCandidate => typeCandidate.CanConvertTo(leftType))
                    .Single();

                return CreateCandidates(leftType);
            }
        }

        class StmtTypeChecker : IStmtComponentVisitor<TypeCheckEnv>, IForInitComponentVisitor<TypeCheckEnv>
        {
            private static StmtTypeChecker checker = new StmtTypeChecker();
            public static void Check(IStmtComponent stmt, TypeCheckEnv env)
            {
                stmt.Visit(checker, env);
            }

            public static void Check(IForInitComponent forInit, TypeCheckEnv env)
            {
                forInit.Visit(checker, env);
            }

            public void Visit(BreakStmt breakStmt, TypeCheckEnv env)
            {
            }

            private void CheckExp(TypeCheckEnv env, IExpComponent exp, IType expectedType)
            {
                IType expType = ExpTypeChecker.Check(exp, env);
                if (expType.CanConvertTo(expectedType))
                    throw new TypeCheckConversionFailed(expType, expectedType);
            }

            public void Visit(DoWhileStmt doWhileStmt, TypeCheckEnv env)
            {
                // 중요!
                // Body는 BlockStmt일때만 Scope가 추가되므로 따로 PushScope를 해주지 않습니다.

                Check(doWhileStmt.Body, env);
                CheckExp(env, doWhileStmt.CondExp, env.BoolType);
            }

            public void Visit(ForStmt forStmt, TypeCheckEnv env)
            {
                env.IDTypeMap.Push();
                Check(forStmt.Initializer, env);
                CheckExp(env, forStmt.CondExp, env.BoolType);
                ExpTypeChecker.Check(forStmt.LoopExp, env);
                Check(forStmt.Body, env);
                env.IDTypeMap.Pop();
            }

            public void Visit(VarDecl varDecl, TypeCheckEnv env)
            {
                IType varType = env.ResolveType(varDecl.Type);

                foreach (var nameAndExp in varDecl.NameAndExps)
                {
                    env.IDTypeMap.Add(nameAndExp.VarName, varType);
                    CheckExp(env, nameAndExp.Exp, varType);
                }
            }

            // forInitComponent
            public void Visit(IExpComponent exp, TypeCheckEnv env)
            {
                ExpTypeChecker.Check(exp, env);
            }

            public void Visit(ReturnStmt returnStmt, TypeCheckEnv env)
            {
                CheckExp(env, returnStmt.ReturnExp, env.ThisFunc.ReturnType);
            }

            public void Visit(WhileStmt whileStmt, TypeCheckEnv env)
            {
                // 중요!
                // 1) while문 도입부에 변수선언이 없고,
                // 2) Body는 BlockStmt일때만 Scope가 추가되므로 따로 PushScope를 해주지 않습니다.

                CheckExp(env, whileStmt.CondExp, env.BoolType);
                Check(whileStmt.Body, env);
            }

            public void Visit(IfStmt ifStmt, TypeCheckEnv env)
            {
                // 1) if문 도입부에 변수 선언이 없으므로 PushScope를 해주지 않습니다
                //    만약 if문에 if (var a = Do() ) {} 구문이 추가된다면 Scope를 추가해야 합니다
                CheckExp(env, ifStmt.CondExp, env.BoolType);
                Check(ifStmt.ThenStmt, env);
                Check(ifStmt.ElseStmt, env);
            }

            public void Visit(ExpStmt expStmt, TypeCheckEnv env)
            {
                ExpTypeChecker.Check(expStmt.Exp, env);
            }

            public void Visit(ContinueStmt continueStmt, TypeCheckEnv env)
            {
            }

            public void Visit(BlockStmt blockStmt, TypeCheckEnv env)
            {
                env.IDTypeMap.Push();
                foreach (var stmt in blockStmt.Stmts)
                    Check(stmt, env);
                env.IDTypeMap.Pop();
            }

            public void Visit(BlankStmt blankStmt, TypeCheckEnv env)
            {
            }

            public void Visit(VarDeclStmt varDeclStmt, TypeCheckEnv env)
            {
                IType varType = env.ResolveType(varDeclStmt.Type);

                foreach (var nameAndExp in varDeclStmt.NameAndExps)
                {
                    env.IDTypeMap.Add(nameAndExp.VarName, varType);
                    CheckExp(env, nameAndExp.Exp, varType);
                }
            }
        }
    }
}
