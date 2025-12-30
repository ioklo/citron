#include "TranslationContext.h"

#include "Infra/Ptr.h"
#include "Infra/Exceptions.h"
#include "Infra/Variants.h"
#include "Infra/Expected.h"

#include "Logging/Logger.h"
#include "RSymbol/RFactory.h"
#include "RSymbol/RFuncDecl.h"
#include "RSymbol/RDecl.h"
#include "RSymbol/RTypeArguments.h"
#include "MIR/MLoc.h"
#include "MIR/MExp.h"

#include "ReExp.h"
#include "IrExp.h"
#include "ImExp.h"

#include "GlobalContext.h"
#include "FuncContext.h"
#include "ScopeContext.h"
#include "BinOpQueryService.h"
#include "SRTFactory.h"

#include "Misc.h"

using namespace std;

namespace Citron {

TranslationContext::TranslationContext(
    const GlobalContextPtr& globalContext, const FuncContextPtr& funcContext, const ScopeContextPtr& scopeContext, 
    const LoggerPtr& logger, const RFactoryPtr& rFactory, const MFactoryPtr& mFactory, const SRTFactoryPtr& srtFactory,
    const BinOpQueryServicePtr& binOpQueryService)
    : globalContext{globalContext}, funcContext{funcContext}, scopeContext{scopeContext}
    , logger{logger}, rFactory{rFactory}, mFactory{mFactory}, srtFactory{srtFactory}
    , binOpQueryService{binOpQueryService}
{
}

TranslationContext TranslationContext::Make(
    NFuncDecl* nFuncDecl, 
    const LoggerPtr& logger,
    const RFactoryPtr& rFactory, const MFactoryPtr& mFactory, const SRTFactoryPtr& srtFactory,
    const BinOpQueryServicePtr& binOpQueryService)
{
    auto globalContext = MakePtr<GlobalContext>();
    auto funcContext = MakePtr<FuncContext_FuncDecl>(nFuncDecl, rFactory, mFactory);
    auto scopeContext = MakePtr<ScopeContext>(funcContext, nullptr, 0, rFactory);

    return { globalContext, funcContext, scopeContext, logger, rFactory, mFactory, srtFactory, binOpQueryService };
}

TranslationContext TranslationContext::MakeNestedScopeContext()
{
    auto newScopeContext = MakePtr<ScopeContext>(funcContext, scopeContext, scopeContext->nestedLoop, rFactory);
    return { globalContext, funcContext, newScopeContext, logger, rFactory, mFactory, srtFactory, binOpQueryService };
}

TranslationContext TranslationContext::MakeNestedLoopScopeContext()
{
    auto newScopeContext = MakePtr<ScopeContext>(funcContext, scopeContext, scopeContext->nestedLoop + 1, rFactory);
    return { globalContext, funcContext, newScopeContext, logger, rFactory, mFactory,  srtFactory, binOpQueryService };
}

TranslationContext TranslationContext::MakeLambdaBodyContext(RFuncReturn&& funcRet, std::vector<RFuncParameter>&& funcParams, bool bLastParamVariadic)
{   
    auto newFuncContext = MakePtr<FuncContext_Lambda>(scopeContext, /*bSeqFunc*/ false, move(funcRet), move(funcParams), bLastParamVariadic);
    auto newScopeContext = MakePtr<ScopeContext>(newFuncContext, nullptr, 0, rFactory);

    return { globalContext, newFuncContext, newScopeContext, logger, rFactory, mFactory, srtFactory, binOpQueryService };
}

expected<RType*, DiagPtr> TranslationContext::TranslateSTypeExpToRType(STypeExp* typeExp)
{
    return scopeContext->TranslateSTypeExpToRType(typeExp);
}

Citron::RType* TranslationContext::GetType(MLoc* loc)
{
    return loc->GetType(*rFactory);
}

RType* TranslationContext::GetType(ReExp* reExp)
{
    return reExp->GetType(*rFactory);
}

Citron::RType* TranslationContext::GetType(MExp* exp)
{
    return exp->GetType(*rFactory);
}

RType* TranslationContext::GetTargetType(IrExp_BoxRef* boxRef)
{
    return boxRef->GetTargetType(*rFactory);
}

MLoc_This* TranslationContext::MakeThisLoc()
{
    return funcContext->MakeThisLoc();
}

expected<MExp*, DiagPtr> TranslationContext::MakeMExp_As(MExp* targetExp, RType* testType)
{
    auto targetType = targetExp->GetType(*rFactory);
    auto targetTypeKind = targetType->GetCustomTypeKind();
    auto testTypeKind = testType->GetCustomTypeKind();

    // 5가지 케이스로 나뉜다
    if (testTypeKind == RCustomTypeKind::Class)
    {
        if (targetTypeKind == RCustomTypeKind::Class)
            return MakeMExp<MExp_ClassAsClass>(targetExp, testType);

        else if (targetTypeKind == RCustomTypeKind::Interface)
            return MakeMExp<MExp_InterfaceAsClass>(targetExp, testType);
        else
            throw NotImplementedException{}; // 에러 처리
    }
    else if (testTypeKind == RCustomTypeKind::Interface)
    {
        if (targetTypeKind == RCustomTypeKind::Class)
            return MakeMExp<MExp_ClassAsInterface>(targetExp, testType);
        else if (targetTypeKind == RCustomTypeKind::Interface)
            return MakeMExp<MExp_InterfaceAsInterface>(targetExp, testType);
        else
            throw NotImplementedException{}; // 에러 처리
    }
    else if (testTypeKind == RCustomTypeKind::EnumElem)
    {
        if (targetTypeKind == RCustomTypeKind::Enum)
            return MakeMExp<MExp_EnumAsEnumElem>(targetExp, testType);
        else
            throw NotImplementedException{}; // 에러 처리
    }
    else
        throw NotImplementedException{}; // 에러 처리
}

bool TranslationContext::CanAccess(RDecl* target)
{
    return funcContext->CanAccess(target);
}

bool TranslationContext::IsSeqFunc()
{
    return funcContext->IsSeqFunc();
}

RFuncReturn TranslationContext::GetUnboundFuncReturn()
{
    return funcContext->GetUnboundFuncReturn();
}

void TranslationContext::SetOpenFuncReturn(RType* retType)
{
    funcContext->SetOpenFuncReturn(retType);
}

NLambdaDeclAndArgs TranslationContext::MakeLambdaDeclAndArgs(std::vector<MStmt*>&& body)
{
    throw NotImplementedException{};
}

RTypeArguments* TranslationContext::MakeTypeArguments(const std::vector<RType*>& items)
{
    return rFactory->MakeTypeArguments(items);
}

RTypeArguments* TranslationContext::MergeTypeArguments(RTypeArguments& typeArgs0, RTypeArguments& typeArgs1)
{
    return rFactory->MergeTypeArguments(typeArgs0, typeArgs1);
}

RType* TranslationContext::MakeVoidType()
{
    return rFactory->MakeVoidType();
}

RType* TranslationContext::MakeBoolType()
{
    return rFactory->MakeBoolType();
}

RType* TranslationContext::MakeIntType()
{
    return rFactory->MakeIntType();
}

RType* TranslationContext::MakeStringType()
{
    return rFactory->MakeStringType();
}

bool TranslationContext::IsListType(RType* type, RType** outItemType)
{
    return rFactory->IsListType(type, outItemType);
}

const vector<BinOpInfo>& TranslationContext::GetBinOpInfos(SBinaryOpKind kind)
{
    return binOpQueryService->GetInfos(kind);
}

RFuncReturn TranslationContext::GetFuncReturn(RFuncDecl& decl, RTypeArguments& typeArgs)
{
    return decl.GetFuncReturn(typeArgs, *rFactory);
}

RFuncParameter TranslationContext::GetFuncParam(RFuncDecl& decl, RTypeArguments& typeArgs, size_t index)
{
    return decl.GetFuncParam(typeArgs, index, *rFactory);
}

RType_Enum* TranslationContext::GetBaseEnumType(RType_EnumElem& enumElemType)
{
    return enumElemType.GetBaseEnumType(*rFactory);
}

expected<ImExp*, DiagPtr> TranslationContext::ResolveIdentifier(const RName& name, RTypeArguments* typeArgs)
{
    // struct S<T>
    // {
    //    struct X<V> { }
    //    void F<U>()
    //    {
    //        X<int> x; // 여기서 (X<>, [int])는? (S<>.X<>, [T, int]) // 컴파일 타임엔 여기까지인 거고, 실행중엔 (S<>.X<>, [T, int])는 S<>.F<>의 타입 컨텍스트(예) [string, bool]
    //    }
    // }

    auto eORMember = scopeContext->ResolveIdentifier(name, typeArgs->GetCount());
    RETURN_ON_ERROR(eORMember)

    if (!*eORMember)
        return unexpected{MakePtr<Error_ResolveIdentifier_NotFound>()};

    return visit([this, typeArgs](auto& rMember) -> ImExp* {
        using T = remove_cvref_t<decltype(rMember)>;

        if constexpr (same_as<T, RMember_LocalVar>)
        {
            return srtFactory->MakeImExp<ImExp_LocalVar>(rMember.type, rMember.name);
        }
        else if constexpr (same_as<T, RMember_GlobalFuncs>)
        {   
            return srtFactory->MakeImExp<ImExp_GlobalFuncs>(rMember.items, typeArgs);
        }
        else if constexpr (same_as<T, RMember_StructVar>)
        {
            assert(typeArgs->GetCount() == 0); // ResolveIdentifier가 typeArgs가 있는데 *var를 돌려줬을리가 없다
            return srtFactory->MakeImExp<ImExp_StructVar>(rMember.decl, rMember.typeArgs, /*hasExplicitInstance*/ false, /*explicitInstance*/ nullptr);
        }
        else if constexpr (same_as<T, RMember_ClassVar>)
        {
            assert(typeArgs->GetCount() == 0);
            return srtFactory->MakeImExp<ImExp_ClassVar>(rMember.decl, rMember.typeArgs, /*hasExplicitInstance*/ false, /*explicitInstance*/ nullptr);
        }
        else if constexpr (same_as<T, RMember_Struct>)
        {   
            auto* mergedTypeArgs = rFactory->MergeTypeArguments(*rMember.outerTypeArgs, *typeArgs);
            return srtFactory->MakeImExp<ImExp_Struct>(rMember.decl, mergedTypeArgs);
        }
        else
        {
            throw NotImplementedException{};
        }
    }, **eORMember);
}

} // namespace Citron