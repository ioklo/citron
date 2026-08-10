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
#include "RSymbol/RTypeArguments.h"
#include "RSymbol/RTypeParam.h"
#include "MIR/MFuncBody.h"
#include "PhaseManager.h"
#include "PostBuildNonTypeSymbolContext.h"
#include "CommonTranslation.h"
#include "SmDeclContext_Decl.h"
#include "SmTypeTranslation.h"
#include "SmTypeTranslationContexts.h"
#include "ImplTraitFuncTask.h"

using namespace std;

namespace Citron {

void ImplTraitTask::Register(SImplTraitDecl* sImplDecl, TakeRef<SmDeclContextPtr> outerDeclContext, RImplTraitDeclOuter rOuter, TakeRef<RFactoryPtr> rFactory, PhaseManager& phaseManager)
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
bool IsSameUnderCorrespondTypeEnv(RType* x, RType* y);

template<typename TRDecl>
bool IsSameUnderCorrespondTypeEnv(RAppliedDecl<TRDecl>& x, RAppliedDecl<TRDecl>& y)
{
    if (x.decl != y.decl)
        return false;

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


bool IsSameUnderCorrespondTypeEnv(RType* x, RType* y)
{
    struct Visitor
    {
        using ResultType = bool;
        RType* rawTypeY;

        bool Visit(RType_Nullable* typeX) 
        {
            auto* typeY = dynamic_cast<RType_Nullable*>(rawTypeY);
            if (!typeY) return false;

            return IsSameUnderCorrespondTypeEnv(typeX->innerType, typeY->innerType);
        }

        bool Visit(RType_NullableInplace* typeX) 
        {
            auto* typeY = dynamic_cast<RType_NullableInplace*>(rawTypeY);
            if (!typeY) return false;

            return IsSameUnderCorrespondTypeEnv(typeX->innerType, typeY->innerType);
        }

        bool Visit(RType_TypeVar* typeX) 
        {
            auto* typeY = dynamic_cast<RType_TypeVar*>(rawTypeY);
            if (!typeY) return false;

            return typeX->typeParam->GetGlobalIndex() == typeY->typeParam->GetGlobalIndex();
        }

        bool Visit(RType_Void* typeX) 
        {
            auto* typeY = dynamic_cast<RType_Void*>(rawTypeY);
            if (!typeY) return false;

            return true;
        }

        bool Visit(RType_Primitive* typeX) 
        {
            return typeX == rawTypeY;
        }

        bool Visit(RType_Tuple* typeX) 
        {
            auto* typeY = dynamic_cast<RType_Tuple*>(rawTypeY);
            if (!typeY) return false;

            size_t countX = typeX->vars.size();
            if (countX != typeY->vars.size())
                return false;

            for (size_t i = 0; i < countX; i++)
            {
                auto& xVar = typeX->vars[i];
                auto& yVar = typeY->vars[i];
             
                if (!IsSameUnderCorrespondTypeEnv(xVar.declType, yVar.declType))
                    return false;
            }

            return true;
        }

        bool Visit(RType_Func* typeX) 
        {
            auto* typeY = dynamic_cast<RType_Func*>(rawTypeY);
            if (!typeY) return false;

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

                if (!IsSameUnderCorrespondTypeEnv(xParam.type, yParam.type))
                    return false;
            }

            if (!IsSameUnderCorrespondTypeEnv(typeX->retType, typeY->retType))
                return false;

            return true;
        }

        bool Visit(RType_Ptr* typeX) 
        {
            auto* typeY = dynamic_cast<RType_Ptr*>(rawTypeY);
            if (!typeY) return false;

            return IsSameUnderCorrespondTypeEnv(typeX->innerType, typeY->innerType);
        }

        bool Visit(RType_Shared* typeX) 
        {
            auto* typeY = dynamic_cast<RType_Shared*>(rawTypeY);
            if (!typeY) return false;

            return IsSameUnderCorrespondTypeEnv(typeX->innerType, typeY->innerType);
        }

        bool Visit(RType_Box* typeX) 
        {
            auto* typeY = dynamic_cast<RType_Box*>(rawTypeY);
            if (!typeY) return false;

            return IsSameUnderCorrespondTypeEnv(typeX->innerType, typeY->innerType);
        }

        bool Visit(RType_Class* typeX) 
        {
            auto* typeY = dynamic_cast<RType_Class*>(rawTypeY);
            if (!typeY) return false;

            return IsSameUnderCorrespondTypeEnv(typeX->appliedDecl, typeY->appliedDecl);
        }

        bool Visit(RType_Struct* typeX) 
        {
            auto* typeY = dynamic_cast<RType_Struct*>(rawTypeY);
            if (!typeY) return false;

            return IsSameUnderCorrespondTypeEnv(typeX->appliedDecl, typeY->appliedDecl);
        }

        bool Visit(RType_Enum* typeX) 
        {
            auto* typeY = dynamic_cast<RType_Enum*>(rawTypeY);
            if (!typeY) return false;

            return IsSameUnderCorrespondTypeEnv(typeX->appliedDecl, typeY->appliedDecl);
        }

        bool Visit(RType_EnumElem* typeX) 
        {
            auto* typeY = dynamic_cast<RType_EnumElem*>(rawTypeY);
            if (!typeY) return false;

            return IsSameUnderCorrespondTypeEnv(typeX->appliedDecl, typeY->appliedDecl);
        }

        bool Visit(RType_Interface* typeX) 
        {
            auto* typeY = dynamic_cast<RType_Interface*>(rawTypeY);
            if (!typeY) return false;

            if (typeX->bLocal != typeY->bLocal)
                return false;

            return IsSameUnderCorrespondTypeEnv(typeX->appliedDecl, typeY->appliedDecl);
        }

        bool Visit(RType_Lambda* typeX) 
        {
            // TODO: [65] 2026-07-06, RLambdaDecl제거, RStructDecl을 쓰도록 변경
            throw NotImplementedException{};
        }

        bool Visit(RType_Opaque* typeX) 
        {
            // TODO: [66] 2026-07-09, Trait, Extend 구현
            throw NotImplementedException{};
        }
    };

    return Accept(Visitor{y}, x);
}

expected<void, DiagPtr> ImplTraitTask::CheckTarget(RStructDecl* rStructTargetDecl, PostBuildNonTypeSymbolContexts& contexts)
{
    // target과 impl은 항상 같은 outer안에 들어 있다는게 보장
   
    // 1. impl의 typeParam 개수도 struct랑 같다는걸 체크 struct<X> ... impl S<T> ... 
    if (rStructTargetDecl->GetTypeParamCount() != rImplTraitDecl->GetTypeParamCount())
        return Error<Error_ImplTrait_MismatchTypeParamCountWithTarget>();

    // TENV(rImplTraitDecl) => appliedTrait
    auto traitOfImplTrait = rImplTraitDecl->GetTrait();
    for (auto& traitOfTarget : rStructTargetDecl->GetTraits()) // tenv: rStructTargetDecl
    {
        // TENV(rStructTargetDecl) => trait
        if (IsSameUnderCorrespondTypeEnv(traitOfTarget, traitOfImplTrait))
            return {};
    }

    return Error<Error_ImplTrait_NoMatchedTraitOfTarget>();
}

bool IsCorrespond(RAppliedDecl<RTraitFuncDecl> traitFunc, RAppliedDecl<RImplTraitFuncDecl> implTraitFunc)
{
    assert(traitFunc.outerTypeArgs->GetCount() == implTraitFunc.outerTypeArgs->GetCount());

    // 1. return type

    
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

expected<void, DiagPtr> ImplTraitTask::CheckTraitConformance(PostBuildNonTypeSymbolContexts& contexts)
{
    // 일단은 함수 signature가 같은지만 확인하면 된다
    auto rTrait = rImplTraitDecl->GetTrait(); // class C<X> { impl S<T, U> : Tr<...> { } } // Tr<...> is under TEnv(C<>.$I<,>(S, Tr<...>), [X, T, U])

    // trait Tr<T, U> { void F<V>(T t); }  // F is under TEnv(Tr<,>.F<>, [T, U, V])
    for (auto rTraitMemberDecl : rTrait.decl->GetMembers())
    {
        auto result = rTraitMemberDecl.Visit([this](auto* rTraitMemberDecl) -> expected<void, DiagPtr> {
            using T = remove_cvref_t<decltype(rTraitMemberDecl)>;

            if constexpr (same_as<T, RTraitFuncDecl*>)
            {
                // RTraitFuncDecl에 대응하는 RImplTraitFuncDecl이 있는지 확인
                // ImplTraitFuncDecl중에 대응하는 return과 parameter가 있는지 확인

                // 일단 이름으로 검색
                RName* name = rTraitMemberDecl->TryGetName();
                assert(name); // func decl은 항상 이름이 있어야 한다

                for (auto& rImplTraitMemberDecl : rImplTraitDecl->GetMembers())
                {   
                    visit([](auto* rImplTraitMemberDecl) {
                        using U = remove_cvref_t<decltype(rImplTraitMemberDecl)>;
                        if constexpr (same_as<U, RImplTraitFuncDecl*>)
                        {

                        }
                        else static_assert(false);
                    });
                }
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