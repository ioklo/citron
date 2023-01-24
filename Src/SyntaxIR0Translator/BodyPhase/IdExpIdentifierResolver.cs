using System;
using System.Collections.Generic;
using Citron.Collections;
using System.Linq;

using S = Citron.Syntax;
using R = Citron.IR0;
using Citron.Infra;
using System.Diagnostics;
using Pretune;
using Citron.Symbol;
using Citron.Syntax;
using System.Reflection;

namespace Citron.Analysis
{
    abstract class IdExpIdentifierResolverException : Exception
    {
        class NotFound : IdExpIdentifierResolverException { }
    }

    // 함수의 경우
    // void F<T, U>(U u) { ... }
    // 
    // F<int>("hi"); // 이럴때, F<int>가 인자로 넘어오면
    // 
    // F<int>의 결과는 ExpResult.GlobalFuncs([DeclAndConstructor(Fd, F)], [int]) <- 아직 Func를 만들지 못한다

    [AutoConstructor]
    partial struct IdExpIdentifierResolver
    {
        readonly static Name thisName = new Name.Normal("this");

        Name idName;
        ImmutableArray<IType> typeArgs;

        ScopeContext context;
        SymbolFactory factory;
        ImmutableArray<ModuleDeclSymbol> modules;

        public ExpResult Resolve() // nothrow
        {
            // 1. local 변수, local 변수에서는 힌트를 쓸 일이 없다
            var localVarInfo = GetLocalVarInfo();
            if (localVarInfo != ExpResults.NotFound) return localVarInfo;

            // 2. this, local변수 this를 만들게 되면 그것보다 뒤에 있다
            if (idName.Equals(thisName) && typeArgs.Length == 0)
            {
                var thisType = context.GetThisType(); // NOTICE: 람다는 여기서 null을 리턴한다

                if (thisType == null)
                    return ExpResults.CantGetThis;

                // 람다 안에 있으면, this는 외부 참조로 동작한다
                return new ExpResult.ThisVar(thisType);
            }

            // 3. contextMember. this멤버이거나, 람다 멤버
            // thisType의 {{instance, static} * {변수, 함수}}, 타입. 아직 지원 안함
            // 힌트는 오버로딩 함수 선택에 쓰일수도 있고,
            // 힌트가 thisType안의 enum인 경우 elem을 선택할 수도 있다
            var contextMemberInfo = GetContextMemberInfo();
            if (contextMemberInfo != ExpResults.NotFound) return contextMemberInfo;

            // 4. 네임스페이스 -> 바깥 네임스페이스 -> module global, 함수, 타입, 
            // 오버로딩 함수 선택, hint가 global enum인 경우, elem선택
            var externalGlobalInfo = GetGlobalInfo();
            if (externalGlobalInfo != ExpResults.NotFound) return externalGlobalInfo;

            return ExpResults.NotFound;
        }

        ExpResult GetLocalVarInfo()
        {
            // 지역 스코프에는 변수만 있고, 함수, 타입은 없으므로 이름이 겹치는 것이 있는지 검사하지 않아도 된다
            if (typeArgs.Length != 0) return ExpResults.NotFound;

            var varInfo = context.GetLocalVarInfo(idName);
            if (varInfo == null) return ExpResults.NotFound;

            return new ExpResult.LocalVar(varInfo.Value.IsRef, varInfo.Value.Type, varInfo.Value.Name);
        }

        // 클래스 멤버 함수였으면 this 멤버변수를, 람다 안이라면 람다의 멤버변수를 리턴한다
        ExpResult GetContextMemberInfo()
        {
            var outerType = context.GetOuterType();
            var result = outerType.QueryMember(idName, typeArgs.Length);

            return MakeExpResult(result);
        }
        
        ExpResult GetGlobalInfo()
        {
            var funcDeclSymbol = context.GetFuncDeclSymbol();      // Body분석이기 때문에
            var curOuterNode = funcDeclSymbol.GetOuterDeclNode()!; // 전역함수도 모듈에 속하기 때문에 null이 아니다

            var candidates = new Candidates<ExpResult>();
            while(curOuterNode != null)
            {
                candidates.Clear();

                // 1. 타입 인자에서 찾기 (타입 인자는 declSymbol이 없으므로 리턴값은 Symbol이어야 한다)
                // T => X<>의 TypeVar T
                if (typeArgs.Length == 0) // optimization, for문 안쪽에 있어야 하는데 뺐다
                {
                    int typeParamCount = curOuterNode.GetTypeParamCount();
                    for (int i = 0; i < typeParamCount; i++)
                    {
                        var typeParam = curOuterNode.GetTypeParam(i);
                        if (typeParam.Equals(idName))
                        {
                            int baseTypeParamIndex = curOuterNode.GetOuterDeclNode()?.GetTotalTypeParamCount() ?? 0;
                            var typeVarType = new TypeVarType(baseTypeParamIndex + i, typeParam);
                            candidates.Add(new ExpResult.TypeVar(typeVarType));
                            break; // 같은 이름이 있을수 없으므로 바로 종료
                        }
                    }
                }

                // class X<T> { class Y<U> {
                //     void F<V, W>(W w) { }
                //     void F<T>() { }
                // }
                //
                // F<int> 는 둘다 지칭 가능하므로 
                // F<int> => (X<T>.Y<U>, [(F, 2, [W]), (F, 1, [])], [int])
                var outerPath = curOuterNode.GetDeclSymbolId().Path;
                foreach (var module in modules)
                {
                    var outerDeclSymbol = (outerPath == null) ? module : module.GetDeclSymbol(outerPath);
                    if (outerDeclSymbol == null) continue;

                    var outerSymbol = outerDeclSymbol.MakeOpenSymbol(factory);
                    var symbolQueryResult = outerSymbol.QueryMember(idName, typeArgs.Length);

                    if (symbolQueryResult is SymbolQueryResult.Valid)
                    {
                        var candidate = MakeExpResult(symbolQueryResult);
                        candidates.Add(candidate);
                    }
                    else if (symbolQueryResult is SymbolQueryResult.NotFound) continue;
                    else if (symbolQueryResult is SymbolQueryResult.Error) // 에러가 났으면 무시하지 말고 리턴
                        return MakeExpResult(symbolQueryResult);
                    else
                        throw new UnreachableCodeException();
                }

                var uniqueResult = candidates.GetUniqueResult();
                if (uniqueResult.IsFound(out var expResult))
                    return expResult;
                else if (uniqueResult.IsMultipleError())
                    return ExpResults.MultipleCandiates;

                // not found인 경우 계속 진행
                Debug.Assert(uniqueResult.IsNotFound());
                curOuterNode = curOuterNode.GetOuterDeclNode()!;
            }

            return ExpResults.NotFound;
        }

        ExpResult MakeExpResult(SymbolQueryResult result)
        {
            switch (result)
            {
                case SymbolQueryResult.Error errorResult:
                    return errorResult.ToErrorIdentifierResult();

                case SymbolQueryResult.NotFound:
                    return ExpResults.NotFound;

                // 여기서부터 case ItemQueryResult.Valid 
                #region Class
                case SymbolQueryResult.Class classResult:
                    return new ExpResult.Class(classResult.ClassConstructor.Invoke(typeArgs));

                case SymbolQueryResult.ClassMemberFuncs classMemberFuncsResult:
                    return new ExpResult.ClassMemberFuncs(classMemberFuncsResult.Infos, typeArgs, false, null);

                case SymbolQueryResult.ClassMemberVar classMemberVarResult:
                    return new ExpResult.ClassMemberVar(classMemberVarResult.Var, false, null);
                #endregion

                #region Struct
                case SymbolQueryResult.Struct structResult:
                    return new ExpResult.Struct(structResult.StructConstructor.Invoke(typeArgs));

                case SymbolQueryResult.StructMemberFuncs structMemberFuncsResult:
                    return new ExpResult.StructMemberFuncs(structMemberFuncsResult.Infos, typeArgs, false, null);

                case SymbolQueryResult.StructMemberVar structMemberVarResult:
                    return new ExpResult.StructMemberVar(structMemberVarResult.Var, false, null);
                #endregion

                #region Enum
                case SymbolQueryResult.Enum enumResult:
                    return new ExpResult.Enum(enumResult.EnumConstructor.Invoke(typeArgs));

                case SymbolQueryResult.EnumElem:
                    throw new NotImplementedException();  // TODO: 무슨 뜻인지 확실히 해야 한다
                #endregion

                case SymbolQueryResult.LambdaMemberVar lambdaMemberVarResult:
                    return new ExpResult.LambdaMemberVar(() => lambdaMemberVarResult.Symbol);

                default:
                    throw new UnreachableCodeException();
            }
        }
    }
}
