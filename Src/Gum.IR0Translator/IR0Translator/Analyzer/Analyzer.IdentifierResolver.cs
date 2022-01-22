using System;
using System.Collections.Generic;
using Gum.Collections;
using System.Linq;

using S = Gum.Syntax;
using M = Gum.CompileTime;
using R = Gum.IR0;
using Gum.Infra;
using System.Diagnostics;
using Pretune;
using Gum.Analysis;

namespace Gum.IR0Translator
{
    partial class Analyzer
    {   
        static IdentifierResult.Error ToErrorIdentifierResult(SymbolQueryResult.Error errorResult)
        {
            switch(errorResult)
            {
                case SymbolQueryResult.Error.MultipleCandidates:
                    return IdentifierResult.Error.MultipleCandiates.Instance;

                case SymbolQueryResult.Error.VarWithTypeArg:
                    return IdentifierResult.Error.VarWithTypeArg.Instance;
            }

            throw new UnreachableCodeException();
        }
        
        [AutoConstructor]
        partial struct IdExpIdentifierResolver
        {
            string idName;
            ImmutableArray<ITypeSymbol> typeArgs;
            ResolveHint hint;

            GlobalContext globalContext;
            ICallableContext callableContext;
            LocalContext localContext;

            public static IdentifierResult Resolve(
                string idName, ImmutableArray<ITypeSymbol> typeArgs, ResolveHint hint,
                GlobalContext globalContext,
                ICallableContext callableContext,
                LocalContext localContext)
            {
                var resolver = new IdExpIdentifierResolver(idName, typeArgs, hint, globalContext, callableContext, localContext);
                return resolver.Resolve();
            }

            IdentifierResult GetLocalVarOutsideLambdaInfo()
            {
                // 지역 스코프에는 변수만 있고, 함수, 타입은 없으므로 이름이 겹치는 것이 있는지 검사하지 않아도 된다
                if (typeArgs.Length != 0) return IdentifierResult.NotFound.Instance;

                var varInfo = callableContext.GetLocalVarOutsideLambda(idName);
                if (varInfo == null) return IdentifierResult.NotFound.Instance;

                return new IdentifierResult.LocalVarOutsideLambda(varInfo.Value.IsRef, varInfo.Value.TypeSymbol, varInfo.Value.Name);
            }

            IdentifierResult GetLocalVarInfo()
            {
                // 지역 스코프에는 변수만 있고, 함수, 타입은 없으므로 이름이 겹치는 것이 있는지 검사하지 않아도 된다
                if (typeArgs.Length != 0) return IdentifierResult.NotFound.Instance;

                var varInfo = localContext.GetLocalVarInfo(idName);
                if (varInfo == null) return IdentifierResult.NotFound.Instance;

                return new IdentifierResult.LocalVar(varInfo.Value.IsRef, varInfo.Value.TypeSymbol, varInfo.Value.Name);
            }

            IdentifierResult GetThisMemberInfo()
            {
                var thisType = callableContext.GetThisType();
                if (thisType == null) return IdentifierResult.NotFound.Instance;

                var itemQueryResult = thisType.QueryMember(new M.Name.Normal(idName), typeArgs.Length);

                switch(itemQueryResult)
                {
                    case SymbolQueryResult.Error errorResult:
                        return ToErrorIdentifierResult(errorResult);

                    case SymbolQueryResult.NotFound:
                        return IdentifierResult.NotFound.Instance;

                    // 여기서부터 case ItemQueryResult.Valid 
                    #region Class
                    case SymbolQueryResult.Class classResult:
                        return new IdentifierResult.Class(classResult.ClassConstructor.Invoke(typeArgs));

                    case SymbolQueryResult.ClassConstructors classConstructorsResult:
                        throw new UnreachableCodeException(); // 이름으로 참조 불가능

                    case SymbolQueryResult.ClassMemberFuncs classMemberFuncsResult:
                        return new IdentifierResult.ClassMemberFuncs(classMemberFuncsResult.FuncConstructors);

                    case SymbolQueryResult.ClassMemberVar classMemberVarResult:
                        return new IdentifierResult.ClassMemberVar(classMemberVarResult.Var);

                    #endregion

                    #region Struct
                    case SymbolQueryResult.Struct structResult:
                        return new IdentifierResult.Struct(structResult.StructConstructor.Invoke(typeArgs));

                    case SymbolQueryResult.StructConstructors structConstructorsResult:
                        throw new UnreachableCodeException(); // 이름으로 참조 불가능                                                              

                    case SymbolQueryResult.StructMemberFuncs structMemberFuncsResult:
                        return new IdentifierResult.StructMemberFuncs(structMemberFuncsResult.FuncConstructors);

                    case SymbolQueryResult.StructMemberVar structMemberVarResult:
                        return new IdentifierResult.StructMemberVar(structMemberVarResult.Var);

                    #endregion

                    #region Enum
                    case SymbolQueryResult.Enum enumResult:
                        return new IdentifierResult.Enum(enumResult.EnumConstructor.Invoke(typeArgs));

                    case SymbolQueryResult.EnumElem:
                        throw new NotImplementedException();  // TODO: 무슨 뜻인지 확실히 해야 한다
                    #endregion
                }

                // TODO: implementation
                return IdentifierResult.NotFound.Instance;
            }

            IdentifierResult GetInternalGlobalVarInfo()
            {
                if (typeArgs.Length != 0) return IdentifierResult.NotFound.Instance;
                
                var varInfo = globalContext.GetInternalGlobalVarInfo(idName);
                if (varInfo == null) return IdentifierResult.NotFound.Instance;

                return new IdentifierResult.GlobalVar(varInfo.IsRef, varInfo.TypeSymbol, varInfo.Name.ToString());
            }
            
            IdentifierResult GetGlobalInfo()
            {
                // TODO: outer namespace까지 다 돌아야 한다
                SymbolPath? namespacePath = null;
                var globalResult = globalContext.QuerySymbol(namespacePath, new M.Name.Normal(idName), typeArgs.Length);

                switch (globalResult)
                {
                    case SymbolQueryResult.NotFound: return IdentifierResult.NotFound.Instance;
                    case SymbolQueryResult.Error errorResult: return ToErrorIdentifierResult(errorResult);

                    // Valid
                    case SymbolQueryResult.Class classResult:
                        return new IdentifierResult.Class(classResult.ClassConstructor.Invoke(typeArgs));

                    case SymbolQueryResult.Struct structResult:
                        return new IdentifierResult.Struct(structResult.StructConstructor.Invoke(typeArgs));

                    case SymbolQueryResult.Enum enumResult:
                        return new IdentifierResult.Enum(enumResult.EnumConstructor.Invoke(typeArgs));

                    case SymbolQueryResult.GlobalFuncs globalFuncsResult:
                        return new IdentifierResult.GlobalFuncs(globalFuncsResult.FuncConstructors);

                    case SymbolQueryResult.EnumElem enumElemResult:                        
                        return new IdentifierResult.EnumElem(enumElemResult.Symbol);

                    // 리턴될 수 없는 것들
                    case SymbolQueryResult.ClassMemberFuncs:
                    case SymbolQueryResult.ClassMemberVar:
                    case SymbolQueryResult.ClassConstructors:
                    case SymbolQueryResult.StructMemberFuncs:
                    case SymbolQueryResult.StructMemberVar:
                    case SymbolQueryResult.StructConstructors:
                        throw new UnreachableCodeException(); // global item도 아닐뿐더러, 이름으로 참조가 불가능하다

                    // 빼먹은 것들 트랩
                    default:
                        throw new UnreachableCodeException();
                }
            }

            IdentifierResult ResolveScope()
            {
                // 0. local 변수, local 변수에서는 힌트를 쓸 일이 없다
                var localVarInfo = GetLocalVarInfo();
                if (localVarInfo != IdentifierResult.NotFound.Instance) return localVarInfo;

                // 1. 람다 바깥의 local 변수
                var localOutsideInfo = GetLocalVarOutsideLambdaInfo();
                if (localOutsideInfo != IdentifierResult.NotFound.Instance) return localOutsideInfo;

                // 2. 'this'
                if (idName == "this" && typeArgs.Length == 0)
                    return IdentifierResult.ThisVar.Instance;

                // 3. thisType의 {{instance, static} * {변수, 함수}}, 타입. 아직 지원 안함
                // 힌트는 오버로딩 함수 선택에 쓰일수도 있고,
                // 힌트가 thisType안의 enum인 경우 elem을 선택할 수도 있다
                var thisMemberInfo = GetThisMemberInfo();
                if (thisMemberInfo != IdentifierResult.NotFound.Instance) return thisMemberInfo;

                // 4. internal global 'variable', 변수이므로 힌트를 쓸 일이 없다
                var internalGlobalVarInfo = GetInternalGlobalVarInfo();
                if (internalGlobalVarInfo != IdentifierResult.NotFound.Instance) return internalGlobalVarInfo;

                // 5. 네임스페이스 -> 바깥 네임스페이스 -> module global, 함수, 타입, 
                // 오버로딩 함수 선택, hint가 global enum인 경우, elem선택
                var externalGlobalInfo = GetGlobalInfo();
                if (externalGlobalInfo != IdentifierResult.NotFound.Instance) return externalGlobalInfo;

                return IdentifierResult.NotFound.Instance;
            }

            IdentifierResult ResolveEnumHint()
            {
                // 힌트가 E고, First가 써져 있으면 E.First를 검색한다
                // enum 힌트 사용, typeArgs가 있으면 지나간다
                if (hint.TypeHint is TypeValueTypeHint typeValueHintType && typeValueHintType.TypeSymbol is EnumSymbol enumTypeValue)
                {
                    // First<T> 같은건 없기 때문에 없을때만 검색한다                    
                    var elemTypeValue = enumTypeValue.GetElement(idName);
                    if (elemTypeValue != null)
                    {
                        // 힌트니까 조건에 맞지않아도 에러를 내지 않고 종료한다
                        if (typeArgs.Length == 0)
                            return new IdentifierResult.EnumElem(elemTypeValue);
                    }
                }
                else if (hint.TypeHint is EnumConstructorTypeHint enumConstructorHint)
                {
                    var elemTypeValue = enumConstructorHint.EnumTypeValue.GetElement(idName);

                    if (elemTypeValue != null)
                    {
                        if (!elemTypeValue.IsStandalone() && typeArgs.Length == 0)
                            return new IdentifierResult.EnumElem(elemTypeValue);
                    }
                }
                
                return IdentifierResult.NotFound.Instance;
            }

            public IdentifierResult Resolve()
            {
                // (로컬/글로벌/멤버)와 enum 힌트는 동급이다
                // E First;
                // E e = First; // 누구 말을 들을것인가, 에러.. local First를 지칭할 방법은? local.First, E.First

                var candidates = new Candidates<IdentifierResult>();

                // 로컬 -> ... -> 글로벌
                var scopeResult = ResolveScope();
                if (scopeResult is IdentifierResult.Valid) candidates.Add(scopeResult);
                if (scopeResult != IdentifierResult.NotFound.Instance) return scopeResult;

                // enum 힌트
                var enumResult = ResolveEnumHint();
                if (enumResult is IdentifierResult.Valid) candidates.Add(scopeResult);
                if (enumResult != IdentifierResult.NotFound.Instance) return enumResult;

                var result = candidates.GetSingle();
                if (result != null) return result;
                if (candidates.IsEmpty) return IdentifierResult.NotFound.Instance;
                if (candidates.HasMultiple) return IdentifierResult.Error.MultipleCandiates.Instance;
                throw new UnreachableCodeException();
            }
        }
    }
}
