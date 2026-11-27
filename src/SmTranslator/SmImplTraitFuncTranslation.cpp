#include "SmImplTraitFuncTranslation.h"
#include "Infra/Ptr.h"
#include "Infra/Expected.h"
#include "Syntax/Syntax.h"
#include "RSymbol/RImplTraitDecl.h"
#include "RSymbol/RImplTraitFuncDecl.h"
#include "RSymbol/RImplTraitMemberDecl.h"
#include "RSymbol/RFactory.h"
#include "MIR/MFuncBody.h"
#include "SmDeclContext_Decl.h"
#include "SmPhase5Contexts.h"
#include "SmPhase7Context.h"
#include "SmPhaseManager.h"
#include "CommonTranslation.h"
#include "ISmPhaseTask.h"
#include "ISmPhase5Task.h"
#include "ISmPhase7Task.h"
#include "SmTypeResolveScope.h"
#include "SmTypeTranslationContexts.h"

using namespace std;

namespace Citron {
namespace {

class SmImplTraitFuncTask
    : public ISmPhaseTask
    , public ISmPhase5Task
    , public ISmPhase7Task
{
    SmDeclContextPtr outerDeclContext;
    RImplTraitDecl* rImplTraitDecl;
    RImplTraitFuncDecl* rImplTraitFuncDecl;
    SImplTraitFuncDecl* sImplTraitFuncDecl;
    RFactoryPtr rFactory;

public:
    SmImplTraitFuncTask(InRefPtr<SmDeclContext> outerDeclContext, RImplTraitDecl* rImplTraitDecl, RImplTraitFuncDecl* rImplTraitFuncDecl, SImplTraitFuncDecl* sImplTraitFuncDecl, InRefPtr<RFactory> rFactory);

    std::expected<void, DiagPtr> RunPhase5(SmPhase5Contexts& context) final;
    std::expected<MFuncBody, DiagPtr> RunPhase7(SmPhase7Context& context) final;
};

} // namespace
} // namespace Citron

namespace Citron {

void SmTranslateImplTraitFunc(InRefPtr<SmDeclContext> outerDeclContext, SImplTraitFuncDecl* sImplTraitFuncDecl, RImplTraitDecl* rImplTraitDecl, InRefPtr<RFactory> rFactory, SmPhaseManager& phaseManager)
{
    // RImplTraitFuncDecl를 만든다
    auto* rImplTraitFuncDecl = rFactory->MakeDecl<RImplTraitFuncDecl>(rImplTraitDecl, /*bSeqFunc*/sImplTraitFuncDecl->bSequence, RName::Normal(sImplTraitFuncDecl->name));
    auto typeParams = MakeTypeParams(rImplTraitDecl->GetAllTypeParamCount(), rImplTraitFuncDecl, sImplTraitFuncDecl->typeParams, rFactory);
    rImplTraitFuncDecl->InitTypeParams(move(typeParams));

    phaseManager.AddTask<SmImplTraitFuncTask>(outerDeclContext, rImplTraitDecl, rImplTraitFuncDecl, sImplTraitFuncDecl, rFactory);
}

namespace {

SmImplTraitFuncTask::SmImplTraitFuncTask(InRefPtr<SmDeclContext> outerDeclContext, RImplTraitDecl* rImplTraitDecl, RImplTraitFuncDecl* rImplTraitFuncDecl, SImplTraitFuncDecl* sImplTraitFuncDecl, InRefPtr<RFactory> rFactory)
    : outerDeclContext{outerDeclContext.Share()}, rImplTraitDecl{rImplTraitDecl}, rImplTraitFuncDecl{rImplTraitFuncDecl}, sImplTraitFuncDecl{sImplTraitFuncDecl}, rFactory{rFactory.Share()}
{
}

std::expected<void, DiagPtr> SmImplTraitFuncTask::RunPhase5(SmPhase5Contexts& context)
{
    auto typeParams = rImplTraitFuncDecl->GetTypeParams();

    SmTypeTranslationContexts contexts{SmTypeResolveScope_DeclHeader{outerDeclContext.get(), typeParams}, rFactory.get()};
    auto e_funcRet = MakeFuncReturn(sImplTraitFuncDecl->funcReturn, rImplTraitFuncDecl, typeParams, contexts);
    RETURN_ON_ERROR(e_funcRet);

    auto e_funcInfo = MakeFuncParameters(sImplTraitFuncDecl->parameters, contexts);
    RETURN_ON_ERROR_REFDECL(e_funcInfo, [funcParams, bLastParamVariadic]);

    auto key = RDeclKey::Func(RName::Normal(sImplTraitFuncDecl->name), funcParams);
    RThisKind thisKind{RThisKind_Ref{rFactory->MakeStructType(rImplTraitDecl->GetTarget())}};

    rImplTraitFuncDecl->Init(move(key), move(*e_funcRet), move(thisKind), move(funcParams), bLastParamVariadic);
    rImplTraitDecl->AddMember(rImplTraitFuncDecl);

    return {};
}

expected<MFuncBody, DiagPtr> SmImplTraitFuncTask::RunPhase7(SmPhase7Context& context)
{
    SmDeclContextPtr declContext = MakePtr<SmDeclContext_Decl<RImplTraitFuncDecl>>(outerDeclContext, rImplTraitFuncDecl, rImplTraitFuncDecl->MakeOpenTypeArgs(*rFactory));
    return context.Translate(move(declContext), rImplTraitFuncDecl, sImplTraitFuncDecl->bSequence, sImplTraitFuncDecl->body);
}

} // namespace
} // namespace Citron