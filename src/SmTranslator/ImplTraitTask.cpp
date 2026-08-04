#include "ImplTraitTask.h"
#include <memory>
#include "Infra/Exceptions.h"
#include "Infra/Expected.h"
#include "Infra/Ptr.h"
#include "Syntax/Syntax.h"
#include "RSymbol/RDecl.h"
#include "RSymbol/RStructDecl.h"
#include "RSymbol/RFactory.h"
#include "RSymbol/RImplTraitDecl.h"
#include "RSymbol/RImplTraitMemberDecl.h"
#include "RSymbol/RImplTraitFuncDecl.h"
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

expected<void, DiagPtr> ImplTraitTask::HandleImplTraitFuncDecl(SImplTraitFuncDecl* sImplTraitFuncDecl, PostBuildNonTypeSymbolContext& context)
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

    RThisKind thisKind{RThisKind_Ref{rFactory->MakeStructType(structTarget, structTarget->MakeOpenTypeArgs(*rFactory))}};

    rImplTraitFuncDecl->Init(move(key), move(typeParams), move(*e_funcRet), move(thisKind), move(funcParams), bLastParamVariadic);
    rImplTraitDecl->AddMember(rImplTraitFuncDecl);

    SmDeclContextPtr implTraitDeclContext = MakePtr<SmDeclContext_Decl<RImplTraitDecl>>(outerDeclContext, rImplTraitDecl, rImplTraitDecl->MakeOpenTypeArgs(*rFactory));
    ImplTraitFuncTask::Register(implTraitDeclContext, rImplTraitFuncDecl, sImplTraitFuncDecl, rFactory, *context.GetPhaseManager());
    return {};
}

expected<void, DiagPtr> ImplTraitTask::PostBuildNonTypeSymbol(PostBuildNonTypeSymbolContext& context)
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
    auto e_trait = context.MakeTrait(sImplDecl->trait, outerDeclContext.get(), typeParams);
    RETURN_ON_ERROR(e_trait);

    auto key = RDeclKey::ImplTrait(rStructTargetDecl, *e_trait);
    rImplTraitDecl->Init(move(key), move(typeParams), *e_trait);

    rOuter.AddImplTrait(rImplTraitDecl);


    // 이제 child 처리
    for (auto& sMemberDecl : sImplDecl->memberDecls)
    {
        auto e_result = visit([this, &context](auto& sMemberDecl) -> expected<void, DiagPtr>{
            using T = remove_cvref_t<decltype(sMemberDecl)>;
            if constexpr (same_as<T, SImplTraitFuncDecl*>)
            {
                // RImplTraitFuncDecl을 만듭니다.
                return HandleImplTraitFuncDecl(sMemberDecl, context);
            }
            else static_assert(false);

        }, sMemberDecl);

        RETURN_ON_ERROR(e_result);
    }

    return {};
}

} // namespace Citron