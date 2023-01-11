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

    [AutoConstructor]
    partial struct IdExpIdentifierResolver
    {
        string idName;
        ImmutableArray<IType> typeArgs;
        IType? hintType;

        ScopeContext context;
        SymbolFactory factory;
        ImmutableArray<ModuleDeclSymbol> modules;

        public static ExpResult Resolve(string idName, ImmutableArray<IType> typeArgs, IType? hintType, ScopeContext context)
        {
            var resolver = new IdExpIdentifierResolver(idName, typeArgs, hintType, context);
            return resolver.Resolve();
        }        

        ExpResult GetLocalVarInfo()
        {
            // 지역 스코프에는 변수만 있고, 함수, 타입은 없으므로 이름이 겹치는 것이 있는지 검사하지 않아도 된다
            if (typeArgs.Length != 0) return ExpResults.NotFound;

            var varInfo = context.GetLocalVarInfo(new Name.Normal(idName));
            if (varInfo == null) return ExpResults.NotFound;

            return new ExpResult.LocalVar(varInfo.Value.IsRef, varInfo.Value.Type, varInfo.Value.Name);
        }

        ExpResult ToExpResult(SymbolQueryResult result)
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

        // 클래스 멤버 함수였으면 this 멤버변수를, 람다 안이라면 람다의 멤버변수를 리턴한다
        ExpResult GetContextMemberInfo()
        {
            var outerType = context.GetOuterType();
            var result = outerType.QueryMember(idName, typeArgs.Length);

            return ToExpResult(result);
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
                
                var outerPath = curOuterNode.GetDeclSymbolId().Path;
                var path = outerPath.Child(idName, typeArgs.Length);

                foreach (var module in modules)
                {
                    var declSymbol = module.GetDeclSymbol(path);

                    if (declSymbol != null)
                    {
                        // class X<T> { class Y<U> { class Z { Y<int> x; } } }
                        // 여기서 Z에 있는 Y<int>는 X<T>.Y<int> 이다
                        // X<T>는 declSymbol의 outer로부터 OpenSymbol을 만들고, Y는 int를 적용해서 만든다

                        // declSymbol이 X<>.Y<> 일때,

                        // outerDeclSymbol은 X<>
                        var outerDeclSymbol = declSymbol.GetOuterDeclNode();

                        // outerSymbol은 X<T> (open)
                        var outerSymbol = outerDeclSymbol?.MakeOpenSymbol(factory);

                        // symbol은 X<T>.Y<int>
                        var symbol = SymbolInstantiator.Instantiate(factory, outerSymbol, declSymbol, typeArgs);
                        var candidate = symbol.MakeExpResult();

                        candidates.Add(candidate);
                    }
                }

                if (candidates.ContainsItem())
                    return candidates;

                curOuterNode = curOuterNode.GetOuterDeclNode()!;

            }



            // TODO: outer namespace까지 다 돌아야 한다
            SymbolPath? namespacePath = null;
            var result = context.QuerySymbol(namespacePath, new Name.Normal(idName), typeArgs.Length);

            return ToExpResult(result);
        }

        ExpResult ResolveScope() // nothrow
        {            
            // 1. local 변수, local 변수에서는 힌트를 쓸 일이 없다
            var localVarInfo = GetLocalVarInfo();
            if (localVarInfo != ExpResults.NotFound) return localVarInfo;

            // 2. this, local변수 this를 만들게 되면 그것보다 뒤에 있다
            if (idName == "this" && typeArgs.Length == 0)
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
        

        public ExpResult Resolve()
        {
            // (로컬/글로벌/멤버)와 enum 힌트는 동급이다
            // E First;
            // E e = First; // 누구 말을 들을것인가, 에러.. local First를 지칭할 방법은? local.First, E.First

            var candidates = new Candidates<ExpResult>();

            // 로컬 -> ... -> 글로벌
            var scopeResult = ResolveScope();
            if (scopeResult is ExpResult.Valid) candidates.Add(scopeResult);
            if (scopeResult != ExpResults.NotFound) return scopeResult;

            var result = candidates.GetSingle();
            if (result != null) return result;
            if (candidates.IsEmpty) return ExpResults.NotFound;
            if (candidates.HasMultiple) return IdentifierResult.Error.MultipleCandiates;
            throw new UnreachableCodeException();
        }
    }
}
