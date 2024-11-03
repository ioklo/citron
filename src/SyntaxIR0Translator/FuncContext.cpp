#include "pch.h"
#include "FuncContext.h"

#include <variant>

#include <Infra/Ptr.h>
#include <Infra/Variants.h>

#include <Syntax/Syntax.h>

#include <IR0/RLambdaMemberVarDecl.h>
#include <IR0/NArgument.h>
#include <IR0/RFuncDecl.h>
#include <IR0/NFuncDeclOuter.h>
#include <IR0/NDecl.h>
#include <IR0/RTypeArguments.h>

#include "TranslationContext.h"
#include "ScopeContext.h"
#include "ImExp.h"

using namespace std;

namespace Citron::SyntaxIR0Translator {

struct IdentifierResolverMultipleCandidatesException
{
    std::vector<ImExpPtr> candidates;
};

// static RName thisName = RName_Normal("this");
FuncContext::FuncContext(const ModuleDeclsPtr& moduleDecls, FuncContextOuter&& outer, bool bSeqFunc, RFuncReturn&& funcReturn, std::vector<RFuncParameter>&& funcParams, bool bLastParamVariadic)
    : moduleDecls(moduleDecls), outer(std::move(outer)), bSeqFunc(bSeqFunc), funcReturn(std::move(funcReturn)), funcParams(std::move(funcParams)), bLastParamVariadic(bLastParamVariadic)
{
}

FuncContextPtr FuncContext::MakeLambdaBodyContext(const ScopeContextPtr& curScopeContext, RFuncReturn&& funcRet, std::vector<RFuncParameter>&& funcParams, bool bLastParamVariadic)
{
    // lambda이므로 outer는 scopeContext이다
    return MakePtr<FuncContext>(moduleDecls, FuncContextOuter_ScopeContext { curScopeContext }, /*bSeqFunc*/ false, std::move(funcRet), std::move(funcParams), bLastParamVariadic);
}

//FuncContextPtr FuncContext::Clone(CloneContext& context)
//{
//    auto* scopeContextOuter = get_if<BodyContextOuter_ScopeContext>(&outer);
//
//    auto newOuterScopeContext = scopeContextOuter ? context.GetClone(scopeContextOuter->scopeContext) : nullptr;
//    auto newBodyContext = MakePtr<BodyContext>(moduleDecls, funcDecl, bSeqFunc, funcReturn, newOuterScopeContext, factory);
//
//    newBodyContext->lambdaMemberVarAndInitArgs = lambdaMemberVarAndInitArgs;
//    newBodyContext->lambdaDecls = lambdaDecls;
//
//    return newBodyContext;
//}
//
//
//void FuncContext::Update(const BodyContextPtr& src, UpdateContext& context)
//{
//    // 안변하는 것들은 assert
//    assert(moduleDecls == src->moduleDecls);
//    assert(funcDecl == src->funcDecl);
//    assert(bSeqFunc == src->bSeqFunc);
//
//    funcReturn = src->funcReturn;
//    context.Update(outerScopeContext, src->outerScopeContext);
//
//    assert(factory == src->factory);
//
//    lambdaMemberVarAndInitArgs = src->lambdaMemberVarAndInitArgs;
//    lambdaDecls = src->lambdaDecls;
//}
    
bool FuncContext::CanAccess(RDecl* target)
{
    // TODO: 현재 scope에서 access check
    return visit(overloaded {
        [target](FuncContextOuter_RFuncDeclOuter& outer) { return outer.decl->GetDecl()->CanAccess(target); },
        [target](FuncContextOuter_ScopeContext& outer) { return outer.scopeContext->funcContext->CanAccess(target); }
    }, outer);
}

// 아직 FuncDecl이 안 만들어진 시기이기 때문에 이 함수가 만들어 질 수가 없다
//RFuncDecl* BodyContext::GetOutermostFuncDecl()
//{
//    return visit(overloaded {
//        [](BodyContextOuter_RFuncDeclOuter& outer) { return this; }, // ???
//        [](BodyContextOuter_ScopeContext& outer) { return outer.scopeContext->bodyContext->GetOutermostFuncDecl(); }
//    }, outer);
//} 


    // 람다일 수도 있고, 함수일 수도 있다
    ImExpPtr ResolveIdentifier(RName&& name, RTypeArguments& typeArgs)
    {
        // 0. 인자 먼저 (ScopeContext에서 이미 검색했으니 스킵)
        // 1. 함수(람다 등) 멤버변수
        // 1. 함수(글로벌, 클래스 멤버, 구초제 멤버) 타입인자
        // 1. 'this' (클래스, 구조체에만 존재)
        class IdentifierResolver
        {
            const RName& name;
            RTypeArguments& typeArgs;
            FuncContext& funcContext;

            vector<ImExpPtr> candidates;
            
            void TryQueryMember(NDecl* curDecl)
            {
                // 1. 타입 인자에서 찾기
                // T => X<>의 TypeVar T
                if (typeArgs.GetCount() == 0)
                {
                    if (auto outerTypeVar = QueryTypeVar(curDecl))
                        candidates.push_back(outerTypeVar);
                }

                // class X<T> { class Y<U> {
                //     void F<V, W>(W w) { }
                //     void F<T>() { }
                // }
                //
                // F<int> 는 둘다 지칭 가능하므로 
                // F<int> => (X<T>.Y<U>, [(F, 2, [W]), (F, 1, [])], [int])
                var outerPath = curDecl.GetDeclSymbolId().Path;
                foreach(var module in funcContext.moduleDeclSymbols)
                {
                    var outerDeclSymbol = module.GetDeclSymbol(outerPath);
                    if (outerDeclSymbol == null) continue;

                    var outerSymbol = outerDeclSymbol.MakeOpenSymbol(funcContext.symbolFactory);
                    var symbolQueryResult = outerSymbol.QueryMember(name, typeArgs.Length);

                    if (symbolQueryResult == null)
                        continue;

                    else if (symbolQueryResult is SymbolQueryResult.MultipleCandidatesError multipleCandidatesError) // 에러가 났으면 무시하지 말고 리턴
                    {
                        var builder = ImmutableArray.CreateBuilder<IntermediateExp>(multipleCandidatesError.Results.Length);
                        foreach(var result in multipleCandidatesError.Results)
                        {
                            var expResult = SymbolQueryResultExpResultTranslator.Translate(result, typeArgs); // NOTICE: 여기서 exception이 발생할 수 있다
                            builder.Add(expResult);
                        }

                        throw IdentifierResolverMultipleCandidatesException(std::move(builder));
                    }
                    else // 에러가 없는 경우
                    {
                        candidates.Clear();
                        var candidate = SymbolQueryResultExpResultTranslator.Translate(symbolQueryResult, typeArgs);
                        candidates.Add(candidate);
                        return;
                    }
                }
            }

            // funcDeclSymbol은 람다나 함수.        
            void TryQueryTypeVar(NDecl* curDecl)
            {   
                if (auto typeVar = QueryTypeVar(curDecl))
                    candidates.push_back(typeVar);
            }

            void TryQueryLambdaMemberVar()
            {
                // 0. lambdaMemberVar를 찾는다
                if (typeArgs.GetCount() != 0) return;

                for(auto& [memberVar, _] : funcContext.lambdaMemberVarAndInitArgs)
                {
                    if (memberVar->name == name)
                    {
                        candidates.push_back(MakePtr<ImExp_LambdaMemberVar>(memberVar, funcContext.openTypeArgs));
                        // 같은 이름이 있을수 없으므로 바로 종료
                        return;
                    }
                }
            }

            ImExpPtr QueryTypeVar(NDecl* curDecl)
            {
                int typeParamCount = curDecl->GetTypeParamCount();
                int baseTypeParamCount = curDecl->GetBaseTypeParamCount();

                for (int i = 0; i < typeParamCount; i++)
                {
                    auto typeParam = curDecl->GetTypeParam(i);
                    if (typeParam == name)
                    {
                        auto typeVarType = context->MakeTypeVarType(baseTypeParamCount + i, typeParam);
                        // 같은 이름이 있을수 없으므로 바로 종료
                        return MakePtr<ImExp_TypeVar>(typeVarType);
                    }
                }

                return nullptr;
            }

            void TryQueryThis()
            {
                static RName thisName = RName_Normal("this");

                // this검색, local변수 this를 만들게 되면 그것보다 뒤에 있다
                if (typeArgs.GetCount() == 0 && name == thisName)
                {
                    auto* funcOuter = funcContext.GetOuterDecl();

                    if (auto* classOuter = dynamic_cast<NClassDecl*>(funcOuter))
                    {
                        auto classType = context.MakeClassType(classOuter, *funcContext.openTypeArgs);
                        candidates.push_back(MakePtr<ImExp_ThisVar>(classType));
                    }
                    else if (auto* structOuter = dynamic_cast<NStructDecl*>(funcOuter))
                    {
                        auto structType = context.MakeStructType(structOuter, *funcContext.openTypeArgs);
                        candidates.push_back(MakePtr<ImExp_ThisVar>(structType));
                    }
                }
            }

        public:
            // 즉시 체인을 빠져나갈땐 exception, 찾지 못해서 다음으로 제어를 넘길땐 null리턴
            // ExpResult.NotFound를 쓰지 않는다
            ImExpPtr Resolve() // throws MultipleCandiatesException
            {
                TryQueryThis();
                TryQueryLambdaMemberVar();
                TryQueryTypeVar(funcContext);

                size_t count = candidates.size();
                if (count == 1) return candidates[0];
                if (1 < count)
                    throw IdentifierResolverMultipleCandidatesException(std::move(candidates));

                // 못찾았으면 아래로 계속 진행
                assert(count == 0);

                // 2. outerScopeContext가 있으면 거기에서, 아니라면 bodyContext의 outer를 찾아본다
                if (!funcContext.GetOuter())
                {
                    auto result = funcContext.outerScopeContext.ResolveIdentifier(name, typeArgs);

                    // 람다 멤버에 없었으므로 (TryQueryLambdaMemberVar) 람다에 추가한다
                    // 로컬과 람다 멤버, this만 감싸는 대상이다
                    switch (result)
                    {
                        case IntermediateExp.LocalVar localResult :
                        {
                            var initExp = new R.LoadExp(new R.LocalVarLoc(localResult.Name), localResult.Type);
                            Debug.Assert(initExp != null);

                            var initArg = new R.Argument.Normal(initExp);
                            var symbol = funcContext.StageLambdaMemberVar(localResult.Type, localResult.Name, initArg);
                            return new IntermediateExp.LambdaMemberVar(symbol);
                        }

                        case IntermediateExp.LambdaMemberVar lambdaMemberResult :
                        {
                            var initExp = new R.LoadExp(new R.LambdaMemberVarLoc(lambdaMemberResult.Symbol), lambdaMemberResult.Symbol.GetDeclType());
                            Debug.Assert(initExp != null);

                            var initArg = new R.Argument.Normal(initExp);
                            var symbol = funcContext.StageLambdaMemberVar(lambdaMemberResult.Symbol.GetDeclType(), lambdaMemberResult.Symbol.GetName(), initArg);
                            return new IntermediateExp.LambdaMemberVar(symbol);
                        }

                        case IntermediateExp.ThisVar thisResult :
                        {
                            // TODO: 워닝, struct의 this는 복사가 일어납니다. 원본과 다를 수 있습니다. ref this로 명시적으로 지정해주세요(?)
                            if (thisResult.Type is StructType)
                                throw new NotImplementedException();

                            var initExp = new R.LoadExp(new R.ThisLoc(), thisResult.Type);
                            Debug.Assert(initExp != null);

                            var initArg = new R.Argument.Normal(initExp);
                            var symbol = funcContext.StageLambdaMemberVar(thisResult.Type, thisName, initArg);
                            return new IntermediateExp.LambdaMemberVar(symbol);
                        }

                        // 나머지는 그대로 리턴
                        default:
                            return result;
                    }
                }

                // 3. 아니라면, funcDecl의 outer에서 찾기 시작한다
                var curOuterDecl = funcContext.funcDeclSymbol.GetOuterDeclNode()!; // 전역함수도 모듈에 속하기 때문에 null이 아니다
                while (curOuterDecl != null)
                {
                    TryQueryMember(curOuterDecl);

                    count = candidates.GetCount();
                    if (count == 1) return candidates.GetAt(0);
                    else if (1 < count)
                    {
                        var builder = ImmutableArray.CreateBuilder<IntermediateExp>(count);
                        for (int i = 0; i < count; i++)
                            builder.Add(candidates.GetAt(i));

                        throw IdentifierResolverMultipleCandidatesException(builder.MoveToImmutable());
                    }

                    // not found인 경우 계속 진행
                    Debug.Assert(count == 0);

                    curOuterDecl = curOuterDecl.GetOuterDeclNode()!;
                    candidates.Clear(); // 재사용
                }

                return null;
            }
        };

        IdentifierResolver resolver { name };
        return resolver.Resolve();

        return new IdentifierResolver(name, typeArgs, this).Resolve();
    }

    public LambdaMemberVarSymbol StageLambdaMemberVar(IType type, Name name, R.Argument initArg)
    {
        var lambdaDeclSymbol = funcDeclSymbol as LambdaDeclSymbol;
        Debug.Assert(lambdaDeclSymbol != null);

        var memberVarDeclSymbol = new LambdaMemberVarDeclSymbol(lambdaDeclSymbol, type, name);
        lambdaMemberVars = lambdaMemberVars.Add(memberVarDeclSymbol);
        lambdaMemberVarInitArgs = lambdaMemberVarInitArgs.Add(initArg);

        return (LambdaMemberVarSymbol)memberVarDeclSymbol.MakeOpenSymbol(symbolFactory);
    }
    
    public void CommitLambdasToDeclSymbolTree()
    {
        foreach(var lambda in lambdaDs)
            funcDeclSymbol.AddLambda(lambda);

        lambdaDs = default;
    }

    public ImmutableArray<R.Argument> MakeLambdaArgs()
    {
        return lambdaMemberVarInitArgs;
    }

    // 리턴값 관련 
    public bool IsSetReturn()
    {
        return bSetReturn;
    }

    // constructor라면 null
    public FuncReturn ? GetReturn()
    {
        Debug.Assert(bSetReturn);
        return funcReturn;
    }

    public void SetReturn(IType retType)
    {
        bSetReturn = true;
        funcReturn = new FuncReturn(retType);
    }
}



} // namespace Citron::SyntaxIR0Translator
