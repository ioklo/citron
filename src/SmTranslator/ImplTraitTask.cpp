#include "ImplTraitTask.h"
#include <memory>
#include "Infra/Exceptions.h"
#include "Infra/Expected.h"
#include "Infra/Ptr.h"
#include "Logging/Diag.h"
#include "Syntax/Syntax.h"
#include "RSymbol/RDecl.h"
#include "RSymbol/RStructDecl.h"
#include "RSymbol/RFactory.h"
#include "RSymbol/RImplTraitDecl.h"
#include "RSymbol/RImplTraitMemberDecl.h"
#include "RSymbol/RImplTraitFuncDecl.h"
#include "RSymbol/RTraitDecl.h"
#include "RSymbol/RTraitFuncDecl.h"
#include "RSymbol/RTypeArguments.h"
#include "RSymbol/RTypeParam.h"
#include "MIR/MFuncBody.h"
#include "SmPhaseManager.h"
#include "PostBuildNonTypeSymbolContext.h"
#include "CommonTranslation.h"
#include "SmDeclContext_Decl.h"
#include "SmTypeTranslation.h"
#include "SmTypeTranslationContexts.h"
#include "ImplTraitFuncTask.h"
#include "SmAppliedDecl.h"
#include "SmType.h"
#include "RAppliedDeclToSmAppliedDecl.h"
#include "RTypeToSmType.h"

using namespace std;

namespace Citron {

void ImplTraitTask::Register(SImplTraitDecl* sImplDecl, TakeRef<SmDeclContextPtr> outerDeclContext, RImplTraitDeclOuter rOuter, TakeRef<RFactoryPtr> rFactory, SmPhaseManager& phaseManager)
{
    shared_ptr<ImplTraitTask> task{new ImplTraitTask{sImplDecl, move(outerDeclContext), rOuter, move(rFactory)}};
    phaseManager.AddPostBuildNonTypeSymbolTask(task);
}

expected<void, DiagPtr> ImplTraitTask::HandleImplTraitFuncDecl(SImplTraitFuncDecl* sImplTraitFuncDecl, PostBuildNonTypeSymbolContexts& context)
{
    // RImplTraitFuncDecl를 만든다
    auto* rImplTraitFuncDecl = rFactory->MakeDecl<RImplTraitFuncDecl>(rImplTraitDecl, /*bSeqFunc*/sImplTraitFuncDecl->bSequence, RName::Normal(sImplTraitFuncDecl->name));

    auto typeParams = MakeTypeParams(rImplTraitDecl->GetAllTypeParamCount(), rImplTraitFuncDecl, sImplTraitFuncDecl->typeParams, rFactory);

    SmTypeTranslationContexts contexts{SmTypeResolveScope_DeclHeader{outerDeclContext.get(), typeParams}, rFactory.get()};
    auto e_funcRet = MakeFuncReturn(sImplTraitFuncDecl->funcReturn, rImplTraitFuncDecl, typeParams, contexts);
    RETURN_ON_ERROR(e_funcRet);

    auto e_funcInfo = MakeFuncParameters(sImplTraitFuncDecl->parameters, contexts);
    RETURN_ON_ERROR_REFDECL(e_funcInfo, [funcParams, bLastParamVariadic]);

    auto key = RDeclKey::Func(RName::Normal(sImplTraitFuncDecl->name), funcParams);

    RThisKind thisKind{RThisKind_Ref{rFactory->MakeStructType(rImplTraitDecl->GetTarget())}};

    rImplTraitFuncDecl->Init(move(key), move(typeParams), move(*e_funcRet), move(thisKind), move(funcParams), bLastParamVariadic);
    rImplTraitDecl->AddMember(rImplTraitFuncDecl);

    SmDeclContextPtr implTraitDeclContext = MakePtr<SmDeclContext_Decl<RImplTraitDecl>>(outerDeclContext, rImplTraitDecl, rImplTraitDecl->MakeOpenTypeArgs(*rFactory));
    ImplTraitFuncTask::Register(implTraitDeclContext, rImplTraitFuncDecl, sImplTraitFuncDecl, rFactory, *context.phaseManager);
    return {};
}

struct SmCorrespondTypeEnv
{
    RTypeArguments* outerTypeArgs;
    RDecl* decl;

    size_t GetLocalIndex(RTypeParam* typeParam)
    {
        size_t count = decl->GetTypeParamCount();
        for (size_t i = 0; i < count; i++)
        {
            auto* localTypeParam = decl->GetTypeParam(i);
            if (localTypeParam == typeParam)
                return i;
        }
        return (size_t)-1;
    }

    RType* GetAppliedType(RTypeParam* typeParam)
    {
        size_t globalIndex = typeParam->GetGlobalIndex();
        return outerTypeArgs->Get(globalIndex);
    }
};

// 둘이 correspond한 typeEnv상에 있다고 가정할때
bool IsCorrespond(RType* x, InRef<optional<SmCorrespondTypeEnv>> o_typeEnvX, RType* y, InRef<optional<SmCorrespondTypeEnv>> o_typeEnvY);

bool IsCorrespond(RAppliedDecl<RTraitDecl>& traitX, RAppliedDecl<RTraitDecl>& traitY)
{
    if (traitX.decl != traitY.decl)
        return false;

    size_t countX = traitX.typeArgs->GetCount();
    assert(countX == traitY.typeArgs->GetCount());

    for (size_t i = 0; i < countX; i++)
    {
        auto* typeArgX = traitX.typeArgs->Get(i);
        auto* typeArgY = traitY.typeArgs->Get(i);

        if (!IsCorrespond(typeArgX, optional<SmCorrespondTypeEnv>{}, typeArgY, optional<SmCorrespondTypeEnv>{}))
            return false;
    }
    return true;
}

template<typename TRDecl>
bool IsCorrespond(RAppliedDecl<TRDecl>& appliedDeclX, InRef<optional<SmCorrespondTypeEnv>> o_typeEnvX, RAppliedDecl<TRDecl>& appliedDeclY, InRef<optional<SmCorrespondTypeEnv>> o_typeEnvY)
{
    if (appliedDeclX.decl != appliedDeclY.decl)
        return false;

    // 같은 decl이면 typeArgs는 같아야 한다
    size_t countX = appliedDeclX.typeArgs->GetCount();
    assert(countX == appliedDeclY.typeArgs->GetCount());

    for (size_t i = 0; i < countX; i++)
    {
        auto* xTypeArg = appliedDeclX.typeArgs->Get(i);
        auto* yTypeArg = appliedDeclY.typeArgs->Get(i);

        if (!IsCorrespond(xTypeArg, o_typeEnvX, yTypeArg, o_typeEnvY))
            return false;
    }
    return true;
}

// 두개
struct TypeCorrespondChecker
{
    using ResultType = bool;
    
    template<typename TRType> requires (!std::same_as<TRType, RType_TypeVar>)
    bool Visit(TRType* typeX, InRef<optional<SmCorrespondTypeEnv>> o_typeEnvX, RType* rawTypeY, InRef<optional<SmCorrespondTypeEnv>> o_typeEnvY)
    {
        // 1. rawTypeY가 TypeVar이 경우
        if (auto* typeVarY = dynamic_cast<RType_TypeVar*>(rawTypeY))
        {
            // 1-1. typeX는 TypeVar가 아닌데, typeVarY는 더 이상 다른 타입으로 변경하지 못한다
            if (!*o_typeEnvY) return false; 

            // 1-2. localIndex라면, typeX는 TypeVar가 아니므로 correspond하지 않는다
            size_t localIndexY = (*o_typeEnvY)->GetLocalIndex(typeVarY->typeParam);
            if (localIndexY != (size_t)-1) return false; 

            auto* appliedTypeY = dynamic_cast<TRType*>((*o_typeEnvY)->GetAppliedType(typeVarY->typeParam));
            if (!appliedTypeY) return false;

            // 적용되면 env가 비어야 한다
            return Visit(typeX, o_typeEnvX, appliedTypeY, optional<SmCorrespondTypeEnv>{});
        }

        auto* typeY = dynamic_cast<TRType*>(rawTypeY);
        if (!typeY) return false;

        return Visit(typeX, o_typeEnvX, typeY, o_typeEnvY);
    }

    bool Visit(RType_TypeVar* typeX, InRef<optional<SmCorrespondTypeEnv>> o_typeEnvX, RType* rawTypeY, InRef<optional<SmCorrespondTypeEnv>> o_typeEnvY)
    {
        // 1. typeEnvX가 없으면
        if (!*o_typeEnvX)
        {
            auto* typeY = dynamic_cast<RType_TypeVar*>(rawTypeY);
            if (!typeY) return false;

            // 1-1. typeEnvY가 없으면, typeVar일때만 가능.
            if (!*o_typeEnvY)
            {
                return typeX == typeY; // 유일하므로, 둘이 같은지만 비교하면 된다.
            }
            else
            {
                size_t localIndexY = (*o_typeEnvY)->GetLocalIndex(typeY->typeParam);
                if (localIndexY != (size_t)-1) return false;

                // applied type도 TypeVar여야 한다
                auto* appliedTypeY = dynamic_cast<RType_TypeVar*>((*o_typeEnvY)->GetAppliedType(typeY->typeParam));
                if (!appliedTypeY) return false;

                return typeX == appliedTypeY;
            }
        }

        // 2. localIndex라면, Y도 localIndex로 변환 가능해야 한다
        size_t localIndexX = (*o_typeEnvX)->GetLocalIndex(typeX->typeParam);
        if (localIndexX != (size_t)-1)
        {
            auto* typeY = dynamic_cast<RType_TypeVar*>(rawTypeY);
            if (!typeY) return false;

            if (!*o_typeEnvY) return false;

            size_t localIndexY = (*o_typeEnvY)->GetLocalIndex(typeY->typeParam);
            if (localIndexY == (size_t)-1) return false;

            return localIndexX == localIndexY;
        }

        // 3. applied type으로 변환해서 비교한다
        auto* appliedTypeX = (*o_typeEnvX)->GetAppliedType(typeX->typeParam);
        return IsCorrespond(appliedTypeX, optional<SmCorrespondTypeEnv>{}, rawTypeY, o_typeEnvY);
    }

    template<typename TRType> requires 
        std::same_as<TRType, RType_Nullable> 
        || std::same_as<TRType, RType_NullableInplace>
        || std::same_as<TRType, RType_Ptr>
        || std::same_as<TRType, RType_Shared>
        || std::same_as<TRType, RType_Box>
    bool Visit(TRType* typeX, InRef<optional<SmCorrespondTypeEnv>> o_typeEnvX, TRType* typeY, InRef<optional<SmCorrespondTypeEnv>> o_typeEnvY)
    {
        return IsCorrespond(typeX->innerType, o_typeEnvX, typeY->innerType, o_typeEnvY);
    }

    // 
    bool Visit(RType_Void* typeX, InRef<optional<SmCorrespondTypeEnv>> o_typeEnvX, RType_Void* typeY, InRef<optional<SmCorrespondTypeEnv>> o_typeEnvY) 
    { 
        return true; 
    }

    bool Visit(RType_Primitive* typeX, InRef<optional<SmCorrespondTypeEnv>> o_typeEnvX, RType_Primitive* typeY, InRef<optional<SmCorrespondTypeEnv>> o_typeEnvY) 
    { 
        return typeX->GetPrimitiveKind() == typeY->GetPrimitiveKind(); 
    }

    bool Visit(RType_Tuple* typeX, InRef<optional<SmCorrespondTypeEnv>> o_typeEnvX, RType_Tuple* typeY, InRef<optional<SmCorrespondTypeEnv>> o_typeEnvY) 
    {
        size_t countX = typeX->vars.size();
        if (countX != typeY->vars.size())
            return false;
        for (size_t i = 0; i < countX; i++)
        {
            auto& xVar = typeX->vars[i];
            auto& yVar = typeY->vars[i];
            if (!IsCorrespond(xVar.declType, o_typeEnvX, yVar.declType, o_typeEnvY))
                return false;
        }
        return true;
    }

    bool Visit(RType_Func* typeX, InRef<optional<SmCorrespondTypeEnv>> o_typeEnvX, RType_Func* typeY, InRef<optional<SmCorrespondTypeEnv>> o_typeEnvY) 
    {
        if (typeX->bLocal != typeY->bLocal)
            return false;
        size_t paramCountX = typeX->params.size();
        if (paramCountX != typeY->params.size())
            return false;
        for (size_t i = 0; i < paramCountX; i++)
        {
            auto& xParam = typeX->params[i];
            auto& yParam = typeY->params[i];
            if (xParam.kind != yParam.kind)
                return false;
            if (!IsCorrespond(xParam.type, o_typeEnvX, yParam.type, o_typeEnvY))
                return false;
        }
        if (!IsCorrespond(typeX->retType, o_typeEnvX, typeY->retType, o_typeEnvY))
            return false;
        return true;
    }

    template<typename TRType> requires 
        std::same_as<TRType, RType_Class>
        || std::same_as<TRType, RType_Struct>
        || std::same_as<TRType, RType_Enum>
        || std::same_as<TRType, RType_EnumElem>
        || std::same_as<TRType, RType_Lambda>
        bool Visit(TRType* typeX, InRef<optional<SmCorrespondTypeEnv>> o_typeEnvX, TRType* typeY, InRef<optional<SmCorrespondTypeEnv>> o_typeEnvY)
    {
        return IsCorrespond(typeX->appliedDecl, o_typeEnvX, typeY->appliedDecl, o_typeEnvY);
    }

    bool Visit(RType_Interface* typeX, InRef<optional<SmCorrespondTypeEnv>> o_typeEnvX, RType_Interface* typeY, InRef<optional<SmCorrespondTypeEnv>> o_typeEnvY) 
    {
        return typeX->bLocal == typeY->bLocal && IsCorrespond(typeX->appliedDecl, o_typeEnvX, typeY->appliedDecl, o_typeEnvY);
    }

    bool Visit(RType_Opaque* typeX, InRef<optional<SmCorrespondTypeEnv>> o_typeEnvX, RType_Opaque* typeY, InRef<optional<SmCorrespondTypeEnv>> o_typeEnvY) 
    {
        return IsCorrespond(typeX->appliedTrait, o_typeEnvX, typeY->appliedTrait, o_typeEnvY) && IsCorrespond(typeX->appliedOwnerFunc, o_typeEnvX, typeY->appliedOwnerFunc, o_typeEnvY);
    }
};

bool IsCorrespond(RType* x, InRef<optional<SmCorrespondTypeEnv>> o_typeEnvX, RType* y, InRef<optional<SmCorrespondTypeEnv>> o_typeEnvY)
{
    return Accept(TypeCorrespondChecker{}, x, o_typeEnvX, y, o_typeEnvY);
}

// struct S<T5> : Tr0, Tr1, ... { ... }
// impl S<T6> : Tr1 { ... }
expected<void, DiagPtr> ImplTraitTask::CheckTarget()
{
    auto& rTarget = rImplTraitDecl->GetTarget();

    // target과 impl은 항상 같은 outer안에 들어 있다는것은 미리 체크
    assert(rTarget.decl->GetOuter() == rImplTraitDecl->GetOuter());
   
    // 1. impl의 typeParam 개수도 struct랑 같다는것도 미리 체크
    assert(rTarget.decl->GetTypeParamCount() == rImplTraitDecl->GetTypeParamCount());

    RAppliedDecl<RTraitDecl> rTraitOfImplTrait = rImplTraitDecl->GetTrait();
    
    // struct의 trait들 중에서 impl이 구현하는 trait를 찾는다
    for (auto& rUnboundTraitOfTarget : rTarget.decl->GetTraits())
    {
        RAppliedDecl<RTraitDecl> rTraitOfTarget = rUnboundTraitOfTarget.Apply(rTarget.typeArgs); // implTrait의 tenv로 맞춤

        if (IsCorrespond(rTraitOfTarget, rTraitOfImplTrait))
            return {};
    }

    return Error<Error_ImplTrait_NoMatchedTraitOfTarget>();
}

// C1<int>.Tr<list<T5>>.F와, C2<T4>.impl_<T5>.F가 correspond한지 확인하는 함수
bool IsCorrespond(ROuterAppliedDecl<RImplTraitFuncDecl>& rImplTraitFunc, ROuterAppliedDecl<RTraitFuncDecl>& rTraitFunc)
{
    // type parameter 개수가 같은지 비교해야 한다
    if (rTraitFunc.decl->GetTypeParamCount() != rImplTraitFunc.decl->GetTypeParamCount())
        return false;

    // RType에 대해서, 
    // outerTypeArgs는 치환하고
    // funcDecl의 typeParam은 따로, index로 비교한다.
    // 이에 대해 별도의 structure를 만들까 하다가, 복잡하지 않으면 일단은 그냥 처리해 본다

    optional<SmCorrespondTypeEnv> rTraitFuncEnv = SmCorrespondTypeEnv{rTraitFunc.outerTypeArgs, rTraitFunc.decl};
    optional<SmCorrespondTypeEnv> rImplTraitFuncEnv = SmCorrespondTypeEnv{rImplTraitFunc.outerTypeArgs, rImplTraitFunc.decl};

    // 1. return type 비교
    RFuncReturn rUnboundTraitFuncRet = rTraitFunc.decl->GetUnboundFuncReturn();
    RFuncReturn rUnboundImplTraitFuncRet = rImplTraitFunc.decl->GetUnboundFuncReturn();

    bool retResult = RFuncReturn::Visit(rUnboundTraitFuncRet, rUnboundImplTraitFuncRet, [&rTraitFuncEnv, &rImplTraitFuncEnv](auto& rTraitFuncRet, auto& rImplTraitFuncRet) -> bool {
        using T1 = remove_cvref_t<decltype(rTraitFuncRet)>;
        using T2 = remove_cvref_t<decltype(rImplTraitFuncRet)>;
        if constexpr (!std::same_as<T1, T2>)
            return false;
        else if constexpr (std::same_as<T1, RFuncReturn_Normal>)
        {   
            return IsCorrespond(rImplTraitFuncRet.type, rImplTraitFuncEnv, rTraitFuncRet.type, rTraitFuncEnv);
        }
        else if constexpr (std::same_as<T1, RFuncReturn_None>)
        {
            return true;
        }
        else if constexpr (std::same_as<T1, RFuncReturn_NotSet>)
        {
            throw NotImplementedException{};
        }
        else static_assert(false);
    });

    // 2. parameter 비교

    // 2-1. parameter 개수 비교
    auto rUnboundTraitFuncParams = rTraitFunc.decl->GetUnboundFuncParams();
    auto rUnboundImplTraitFuncParams = rImplTraitFunc.decl->GetUnboundFuncParams();

    size_t rUnboundTraitFuncParamCount = rUnboundTraitFuncParams.size();

    if (rUnboundTraitFuncParamCount != rUnboundImplTraitFuncParams.size())
        return false;

    for (size_t i = 0; i < rUnboundTraitFuncParamCount; i++)
    {
        auto& rUnboundTraitFuncParam = rUnboundTraitFuncParams[i];
        auto& rUnboundImplTraitFuncParam = rUnboundImplTraitFuncParams[i];

        if (rUnboundTraitFuncParam.kind != rUnboundImplTraitFuncParam.kind)
            return false;

        if (!IsCorrespond(rUnboundImplTraitFuncParam.type, rImplTraitFuncEnv, rUnboundTraitFuncParam.type, rTraitFuncEnv))
            return false;

        // 이름은 보지 않는다
    }

    return true;
}

//bool IsCorrespond(RImplTraitFuncDecl* rImplTraitFuncDecl, RTraitFuncDecl* rTraitFuncDecl, PostBuildNonTypeSymbolContexts& contexts)
//{
//    // 이름이 같으면, signature가 같은지 확인
//    auto traitFunc = TranslateRAppliedDeclToSmAppliedDecl(RAppliedDecl<RTraitFuncDecl>{rTraitFuncDecl, rTraitFuncDecl->MakeOpenTypeArgs(*contexts.rFactory)}, contexts.smFactory);
//    auto implTraitFunc = TranslateRAppliedDeclToSmAppliedDecl(RAppliedDecl<RImplTraitFuncDecl>{rImplTraitFuncDecl, rImplTraitFuncDecl->MakeOpenTypeArgs(*contexts.rFactory)}, contexts.smFactory);
//
//    if (!IsCorrespond(*e_traitFunc, *e_implTraitFunc))
//        return Error<Error_ImplTrait_MismatchSignatureWithCorrespondingTraitFunc>();
//
//    return {};
//}

// TODO: [77] 2026-08-11, trait-impl 효율적으로 검색하기
// rImplTraitDecl에, rTraitFuncDecl이 있는지 확인한다
bool HasCorrespondFunc(RAppliedDecl<RImplTraitDecl>& rImplTrait, ROuterAppliedDecl<RTraitFuncDecl>& rTraitFunc)
{
    // 일단 이름으로 검색
    RName* name = rTraitFunc.decl->TryGetName();
    assert(name); // func decl은 항상 이름이 있어야 한다
    
    for (auto& rImplTraitMemberDecl : rImplTrait.decl->GetMembers())
    {
        auto result = rImplTraitMemberDecl.Visit([name, &rImplTrait, &rTraitFunc](auto* rImplTraitMemberDecl) -> bool {
            using T = remove_cvref_t<decltype(rImplTraitMemberDecl)>;
            if constexpr (same_as<T, RImplTraitFuncDecl*>)
            {
                auto* memberName = rImplTraitMemberDecl->TryGetName();
                if (!memberName) return false;
                if (*memberName != *name) return false;

                // rImplTraitFuncDecl은 적용되지 않은 상태의 decl이고
                ROuterAppliedDecl<RImplTraitFuncDecl> rImplTraitFunc{rImplTrait.typeArgs, rImplTraitMemberDecl};
                return IsCorrespond(rImplTraitFunc, rTraitFunc);
            }
            else static_assert(false);
        });

        if (result) return true;
    }

    return false;
}

expected<void, DiagPtr> ImplTraitTask::CheckTraitConformance()
{
    // 일단은 함수 signature가 같은지만 확인하면 된다
    // class C1<T1> { trait Tr<T2> { void F<T3>(T1 t1, T2 t2, T3* t3); } }
    // class C2<T4> { impl S<T5> : C1<int>.Tr<list<T5>> { void F<T6>(int i, list<T5> l, T6* t6) { ... } } }
    // 
    // C1<int>.Tr<list<T5>> === C1<T1>.Tr<T2> [T1 => int, T2 => list<T5>][T4 => T4, T5 => T5]
    //                      === C1<T1>.Tr<T2> [T1 => int, T2 => list<T5>] (merge)
    //
    // C1<int>.Tr<list<T5>>.F === ^T3. { Ret = void, Params = [T1, T2, T3*] }
    // (C1<T1>.Tr<T2> [T1 => int, T2 => list<T5>]).F === C1<T1>.Tr<T2>.F [T1 => int, T2 => list<T5>]
    //                                               === ^T3. { Ret = void, Params = [T1, T2, T3*] } [T1 => int, T2 => list<T5>]
    //
    // C2<T4>.impl_<T5>.F === ^T6. { Ret = void, Params = [int, list<T5>, T6*] }
    // (C2<T4>.impl_<T5> [T4 => T4, T5 => T5]).F === C2<T4>.impl_<T5>.F [T4 => T4, T5 => T5]
    //                                           === ^T6. { Ret = void, Params = [int, list<T5>, T6*] } [T4 => T4, T5 => T5]

    // 
    // (A) Correspond(^T3. { Ret = void, Params = [T1, T2, T3*] } [T1 => int, T2 => list<T5>], ^T6. { Ret = void, Params = [int, list<T5>, T6*] } [T4 => T4, T5 => T5])
    // (B) Correspond({ Ret = void, Params = [T1, T2, T3*] } [T1 => int, T2 => list<T5>][T3 => $0], { Ret = void, Params = [int, list<T5>, T6*] } [T4 => T4, T5 => T5][T6 => $0])
    //   === Correspond(Ret[T1 => int, T2 => list<T5>][T3 => $0], Ret[T4 => T4, T5 => T5][T6 => $0]) 
    //    && Correspond(Params[T1 => int, T2 => list<T5>][T3 => $0], Params[T4 => T4, T5 => T5][T6 => $0])
    // 
    // (A)꼴은 Correspond(ROuterAppliedDecl<RTraitFuncDecl>, ROuterAppliedDecl<RImplTraitFuncDecl>)
    // (B)꼴은 Correspond(RTraitFuncDecl, SmTypeEnv(RTypeArguments*, RTypeParam* => size_t), RImplTraitFuncDecl, SmTypeEnv(RTypeArguments*, RTypeParam* => size_t))
    //         RTypeArguments에 나타나는 RTypeParam*과, RTypeParam* => size_t의 RTypeParam은 겹치지 않는다.

    RAppliedDecl<RImplTraitDecl> rImplTrait{rImplTraitDecl, rImplTraitDecl->MakeOpenTypeArgs(*rFactory)};
    auto rTrait = rImplTraitDecl->GetTrait();

    // trait의 각 member에 해당하는 조건이 있는지 확인한다
    for (auto rTraitMemberDecl : rTrait.decl->GetMembers())
    {
        // 조건이 만족되었다면
        auto satisfied = rTraitMemberDecl.Visit([this, &rTrait, &rImplTrait](auto* rTraitMemberDecl) -> bool {
            using T = remove_cvref_t<decltype(rTraitMemberDecl)>;

            if constexpr (same_as<T, RTraitFuncDecl*>)
            {
                // RTraitFuncDecl에 대응하는 RImplTraitFuncDecl이 있는지 확인
                // ImplTraitFuncDecl중에 대응하는 return과 parameter가 있는지 확인
                ROuterAppliedDecl<RTraitFuncDecl> rTraitFunc{rTrait.typeArgs, rTraitMemberDecl};
                return HasCorrespondFunc(rImplTrait, rTraitFunc);
            }
            else static_assert(false); 
        });

        if (!satisfied)
            return Error<Error_ImplTrait_TraitContractNotSatisfied>();
    }

    return {};
}

expected<void, DiagPtr> ImplTraitTask::PostBuildNonTypeSymbol(PostBuildNonTypeSymbolContexts& contexts)
{
    // SImplTraitDecl을 RImplTrait로 만든다
    // impl S<T, U> : TraitName
    rImplTraitDecl = rFactory->MakeDecl<RImplTraitDecl>();

    auto* rOuterDecl = rOuter.GetDecl();

    // type parameter 처리
    auto typeParams = MakeTypeParams(rOuterDecl->GetAllTypeParamCount(), rImplTraitDecl, sImplDecl->typeParams, rFactory);

    // impl의 name은 다른 부분과 다르게 현재 scope에 있는 struct/class/enum 이름이다
    auto* rTargetTypeDecl = rOuterDecl->GetTypeMember(RName::Normal(sImplDecl->name));
    if (!rTargetTypeDecl) return Error<Error_ImplTrait_TargetNotFound>();

    assert(rTargetTypeDecl->RTypeDecl_GetDecl()->GetOuter() == rOuterDecl);

    // TODO: [70] 2026-07-15, struct 이외에 class, enum에도 impl 넣기
    auto* rStructTargetDecl = dynamic_cast<RStructDecl*>(rTargetTypeDecl);
    assert(rStructTargetDecl);

    // typeParams를 적용해서, rStructTarget을 만들기
    if (rStructTargetDecl->GetTypeParamCount() != typeParams.size())
        return Error<Error_ImplTrait_MismatchTypeParamCountWithTarget>();

    auto* outerTypeArgs = rOuterDecl->MakeOpenTypeArgs(*rFactory);
    vector<RType*> curTypeArgs;
    curTypeArgs.reserve(typeParams.size());
    for(auto* typeParam : typeParams)
        curTypeArgs.push_back(rFactory->MakeTypeVarType(typeParam));
    auto* targetTypeArgs = rFactory->AppendTypeArguments(outerTypeArgs, curTypeArgs);

    RAppliedDecl<RStructDecl> rTarget{rStructTargetDecl, targetTypeArgs};

    // trait 얻기
    auto e_trait = contexts.MakeTrait(sImplDecl->trait, outerDeclContext.get(), typeParams);
    RETURN_ON_ERROR(e_trait);

    auto key = RDeclKey::ImplTrait(rStructTargetDecl, *e_trait);
    rImplTraitDecl->Init(move(key), move(typeParams), move(rTarget), *e_trait);
    rOuter.AddImplTrait(rImplTraitDecl);

    auto e_verifyResult = CheckTarget();
    RETURN_ON_ERROR(e_verifyResult);
    
    // 이제 child 처리
    for (auto& sMemberDecl : sImplDecl->memberDecls)
    {
        auto e_result = visit([this, &contexts](auto& sMemberDecl) -> expected<void, DiagPtr>{
            using T = remove_cvref_t<decltype(sMemberDecl)>;
            if constexpr (same_as<T, SImplTraitFuncDecl*>)
            {
                // RImplTraitFuncDecl을 만듭니다.
                return HandleImplTraitFuncDecl(sMemberDecl, contexts);
            }
            else static_assert(false);

        }, sMemberDecl);

        RETURN_ON_ERROR(e_result);
    }

    // 모든 child를 다 추가했으면, 이제 conformance check
    auto e_result = CheckTraitConformance();
    RETURN_ON_ERROR(e_result);

    return {};
}

} // namespace Citron