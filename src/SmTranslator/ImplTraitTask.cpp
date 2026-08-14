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

    // thisKind
    auto* structTarget = dynamic_cast<RStructDecl*>(rImplTraitDecl->GetTarget());
    assert(structTarget); // TODO: [70] 2026-07-15, struct 이외에 class, enum에도 impl 넣기

    RThisKind thisKind{RThisKind_Ref{rFactory->MakeStructType(RAppliedDecl<RStructDecl>{structTarget, structTarget->MakeOpenTypeArgs(*rFactory)})}};

    rImplTraitFuncDecl->Init(move(key), move(typeParams), move(*e_funcRet), move(thisKind), move(funcParams), bLastParamVariadic);
    rImplTraitDecl->AddMember(rImplTraitFuncDecl);

    SmDeclContextPtr implTraitDeclContext = MakePtr<SmDeclContext_Decl<RImplTraitDecl>>(outerDeclContext, rImplTraitDecl, rImplTraitDecl->MakeOpenTypeArgs(*rFactory));
    ImplTraitFuncTask::Register(implTraitDeclContext, rImplTraitFuncDecl, sImplTraitFuncDecl, rFactory, *context.phaseManager);
    return {};
}

// 둘이 correspond한 typeEnv상에 있다고 가정할때
bool IsSameUnderCorrespondTypeEnv(SmType* x, SmType* y);

template<typename TRDecl>
bool IsSameUnderCorrespondTypeEnv(SmAppliedDecl<TRDecl>& x, SmAppliedDecl<TRDecl>& y)
{
    if (x.decl != y.decl)
        return false;

    // 같은 decl이면 typeArgs는 같아야 한다
    size_t countX = x.typeArgs.size();
    assert(countX == y.typeArgs.size());

    for (size_t i = 0; i < countX; i++)
    {
        auto* xTypeArg = x.typeArgs[i];
        auto* yTypeArg = y.typeArgs[i];

        if (!IsSameUnderCorrespondTypeEnv(xTypeArg, yTypeArg))
            return false;
    }
    return true;
}

bool IsSameUnderCorrespondTypeEnv(SmType* x, SmType* y)
{
    return SmType::Visit(*x, *y, [](auto& x, auto& y) {
        using X = remove_cvref_t<decltype(x)>;
        using Y = remove_cvref_t<decltype(y)>;

        if constexpr (!same_as<X, Y>)
        {
            return false;
        }
        else if constexpr (same_as<X, SmType_Nullable> || same_as<X, SmType_NullableInplace> || same_as<X, SmType_Ptr> || same_as<X, SmType_Shared> || same_as<X, SmType_Box>)
        {
            return IsSameUnderCorrespondTypeEnv(x.innerType, y.innerType);
        }
        else if constexpr (same_as<X, SmType_TypeVar>)
        {
            return x.index == y.index;
        }
        else if constexpr (same_as<X, SmType_Void>)
        {
            return true;
        }
        else if constexpr (same_as<X, SmType_Primitive>)
        {
            return x.kind == y.kind;
        }
        else if constexpr (same_as<X, SmType_Tuple>)
        {  
            return true;
            size_t countX = x.vars.size();
            if (countX != y.vars.size())
                return false;
            for (size_t i = 0; i < countX; i++)
            {
                auto& xVar = x.vars[i];
                auto& yVar = y.vars[i];
                if (!IsSameUnderCorrespondTypeEnv(xVar.declType, yVar.declType))
                    return false;
            }
            return true;
        }
        else if constexpr (same_as<X, SmType_Func>)
        {
            if (x.bLocal != y.bLocal)
                return false;
            size_t paramCountX = x.params.size();
            if (paramCountX != y.params.size())
                return false;
            for (size_t i = 0; i < paramCountX; i++)
            {
                auto& xParam = x.params[i];
                auto& yParam = y.params[i];
                if (xParam.kind != yParam.kind)
                    return false;
                if (!IsSameUnderCorrespondTypeEnv(xParam.type, yParam.type))
                    return false;
            }
            if (!IsSameUnderCorrespondTypeEnv(x.retType, y.retType))
                return false;
            return true;
        }
        else if constexpr (same_as<X, SmType_Class> || same_as<X, SmType_Struct> || same_as<X, SmType_Enum> || same_as<X, SmType_EnumElem> || same_as<X, SmType_Lambda>)
        {
            return IsSameUnderCorrespondTypeEnv(x.appliedDecl, y.appliedDecl);
        }
        else if constexpr (same_as<X, SmType_Interface>)
        {
            return x.bLocal == y.bLocal && IsSameUnderCorrespondTypeEnv(x.appliedDecl, y.appliedDecl);
        }
        else if constexpr (same_as<X, SmType_Opaque>)
        {
            return IsSameUnderCorrespondTypeEnv(x.appliedTrait, y.appliedTrait) && IsSameUnderCorrespondTypeEnv(x.appliedOwnerFunc, y.appliedOwnerFunc);
        }
        else static_assert(false);
    });
}

expected<void, DiagPtr> ImplTraitTask::CheckTarget(RStructDecl* rStructTargetDecl, PostBuildNonTypeSymbolContexts& contexts)
{
    // target과 impl은 항상 같은 outer안에 들어 있다는게 보장
   
    // 1. impl의 typeParam 개수도 struct랑 같다는걸 체크 struct<X> ... impl S<T> ... 
    if (rStructTargetDecl->GetTypeParamCount() != rImplTraitDecl->GetTypeParamCount())
        return Error<Error_ImplTrait_MismatchTypeParamCountWithTarget>();

    // TENV(rImplTraitDecl) => appliedTrait
    auto rTraitOfImplTrait = rImplTraitDecl->GetTrait();

    // RAppliedDecl -> SmAppliedDecl 
    auto traitofImplTrait = TranslateRAppliedDeclToSmAppliedDecl(rTraitOfImplTrait, contexts.smFactory);

    for (auto& rTraitOfTarget : rStructTargetDecl->GetTraits()) // tenv: rStructTargetDecl
    {
        auto traitOfTarget = TranslateRAppliedDeclToSmAppliedDecl(rTraitOfTarget, contexts.smFactory);
        
        if (IsSameUnderCorrespondTypeEnv(traitOfTarget, traitofImplTrait))
            return {};
    }

    return Error<Error_ImplTrait_NoMatchedTraitOfTarget>();
}

expected<bool, DiagPtr> IsCorrespond(RAppliedDecl<RTraitFuncDecl> rTraitFunc, RAppliedDecl<RImplTraitFuncDecl> rImplTraitFunc, PostBuildNonTypeSymbolContexts& contexts)
{
    assert(rTraitFunc.typeArgs->GetCount() == rImplTraitFunc.typeArgs->GetCount());

    // 1. return type 비교
    auto rTraitFuncRet = rTraitFunc.decl->GetUnboundFuncReturn();
    auto rImplTraitFuncRet = rImplTraitFunc.decl->GetUnboundFuncReturn();

    RFuncReturn::Visit(rTraitFuncRet, rImplTraitFuncRet, [&contexts](auto& rTraitFuncRet, auto& rImplTraitFuncRet) {
        using T1 = remove_cvref_t<decltype(rTraitFuncRet)>;
        using T2 = remove_cvref_t<decltype(rImplTraitFuncRet)>;
        if constexpr (!std::same_as<T1, T2>)
            return false;
        else if constexpr (std::same_as<T1, RFuncReturn_Normal>)
        {
            auto e_traitFuncRetType = TranslateRTypeToSmType(rTraitFuncRet.type, contexts.smFactory);

            auto e_implTraitFuncRetType = TranslateRTypeToSmType(rImplTraitFuncRet.type, contexts.smFactory);

            return IsSameUnderCorrespondTypeEnv(rTraitFuncRet.type, rImplTraitFuncRet.type);
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
    
    // 같은 decl이면 typeArgs는 같아야 한다
    size_t countX = x.typeArgs->GetCount();
    assert(countX == y.typeArgs->GetCount());
    for (size_t i = 0; i < countX; i++)
    {
        auto* xTypeArg = x.typeArgs->Get(i);
        auto* yTypeArg = y.typeArgs->Get(i);
        if (!IsSameUnderCorrespondTypeEnv(xTypeArg, yTypeArg))
            return false;
    }
    return true;
}

bool IsCorrespond(RImplTraitFuncDecl* rImplTraitFuncDecl, RTraitFuncDecl* rTraitFuncDecl, PostBuildNonTypeSymbolContexts& contexts)
{
    // 이름이 같으면, signature가 같은지 확인
    auto traitFunc = TranslateRAppliedDeclToSmAppliedDecl(RAppliedDecl<RTraitFuncDecl>{rTraitFuncDecl, rTraitFuncDecl->MakeOpenTypeArgs(*contexts.rFactory)}, contexts.smFactory);
    auto implTraitFunc = TranslateRAppliedDeclToSmAppliedDecl(RAppliedDecl<RImplTraitFuncDecl>{rImplTraitFuncDecl, rImplTraitFuncDecl->MakeOpenTypeArgs(*contexts.rFactory)}, contexts.smFactory);

    if (!IsCorrespond(*e_traitFunc, *e_implTraitFunc))
        return Error<Error_ImplTrait_MismatchSignatureWithCorrespondingTraitFunc>();

    return {};
}

// TODO: [77] 2026-08-11, trait-impl 효율적으로 검색하기
bool FindCorrespondFunc(RImplTraitDecl* rImplTraitDecl, RName& name)
{
    for (auto& rImplTraitMemberDecl : rImplTraitDecl->GetMembers())
    {
        auto result = visit([&name](auto* rImplTraitMemberDecl) -> bool {
            using T = remove_cvref_t<decltype(rImplTraitMemberDecl)>;
            if constexpr (same_as<T, RImplTraitFuncDecl*>)
            {
                auto* memberName = rImplTraitFuncDecl->TryGetName();
                if (!memberName) return false;
                if (*memberName != name) return false;

                // rImplTraitFuncDecl은 적용되지 않은 상태의 decl이고,

                return IsCorrespond(rImplTraitFuncDecl, rTraitFuncDecl, contexts);
            }
            else static_assert(false);
        }, rImplTraitMemberDecl);

        if (result) return true;
    }
}

expected<void, DiagPtr> ImplTraitTask::CheckTraitConformance(PostBuildNonTypeSymbolContexts& contexts)
{
    // 일단은 함수 signature가 같은지만 확인하면 된다
    // trait Tr<T1> { void F<T2>(T1 t1, T2 t2); }
    // 
    // class C<T3> { impl S<T4, T5> : Tr<list<T4>> { } } 
    // Tr<list<T4>> means [T3, T4, T5] => Tr<T1> { T1 => list<T4> } // 여기서 T1은 RTypeParam, T3는 SmTypeVar
    // SmTypeEnv => Tr<RTypeParams...> { RTypeParam => SmType }
    // SmTypeEnv => RDecl { SmTypeArgs } // SmTypeEnv는 index기반이라 총 갯수만 갖고 있다 (implementation detail)

    // SmTypeEnv => Tr<T1> { T1 => list<$2> } 가 최종

    // Tr<list<T4>> 상태인데, SmTypeEnv => Tr<T1> { T1 => list<$2> } 로 바꾼다
    // RAppliedDecl{Tr<T1>, { T1 => list<T4> }} --> SmAppliedDecl{Tr<T1>, { T1 => list<$2> }} // $2는 C<T3>의 type parameter
    auto rTrait = rImplTraitDecl->GetTrait();
    auto trait = TranslateRAppliedDeclToSmAppliedDecl(rTrait, contexts.smFactory);

    // trait Tr<T, U> { void F<V>(T t); }  // F is under TEnv(Tr<,>.F<>, [T, U, V])
    for (auto rTraitMemberDecl : rTrait.decl->GetMembers())
    {
        auto result = rTraitMemberDecl.Visit([this](auto* rTraitMemberDecl) -> expected<void, DiagPtr> {
            using T = remove_cvref_t<decltype(rTraitMemberDecl)>;

            if constexpr (same_as<T, RTraitFuncDecl*>)
            {
                // [$0, $1, $2] => Tr<T1> { T1 => list<$2> } 인데
                // [$0, $1, $2, $3] => T1<T1>.F<T2> { T1 => list<$2>, T2 => $3 } 을 얻어야 한다

                // RTraitFuncDecl에 대응하는 RImplTraitFuncDecl이 있는지 확인
                // ImplTraitFuncDecl중에 대응하는 return과 parameter가 있는지 확인

                // 일단 이름으로 검색
                RName* name = rTraitMemberDecl->TryGetName();
                assert(name); // func decl은 항상 이름이 있어야 한다

                FindCorrespondFunc(rImplTraitDecl, *name);
            }
            else static_assert(false); 
        });
    }
    
}

expected<void, DiagPtr> ImplTraitTask::PostBuildNonTypeSymbol(PostBuildNonTypeSymbolContexts& contexts)
{
    // SImplTraitDecl을 RImplTrait로 만든다
    // impl S<T, U> : TraitName

    auto* rOuterDecl = rOuter.GetDecl();
    // impl의 name은 다른 부분과 다르게 현재 scope에 있는 struct/class/enum 이름이다
    auto* rTargetDecl = rOuterDecl->GetTypeMember(RName::Normal(sImplDecl->name));

    // TODO: [70] 2026-07-15, struct 이외에 class, enum에도 impl 넣기
    auto* rStructTargetDecl = dynamic_cast<RStructDecl*>(rTargetDecl);
    assert(rStructTargetDecl);

    rImplTraitDecl = rFactory->MakeDecl<RImplTraitDecl>(rStructTargetDecl);

    // type parameter 처리
    auto typeParams = MakeTypeParams(rOuterDecl->GetAllTypeParamCount(), rImplTraitDecl, sImplDecl->typeParams, rFactory);

    // trait 얻기
    auto e_trait = contexts.MakeTrait(sImplDecl->trait, outerDeclContext.get(), typeParams);
    RETURN_ON_ERROR(e_trait);

    auto key = RDeclKey::ImplTrait(rStructTargetDecl, *e_trait);
    rImplTraitDecl->Init(move(key), move(typeParams), *e_trait);
    rOuter.AddImplTrait(rImplTraitDecl);

    auto e_verifyResult = CheckTarget(rStructTargetDecl, contexts);
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
    auto e_result = CheckTraitConformance(contexts);
    RETURN_ON_ERROR(e_result);

    return {};
}

} // namespace Citron