#include "SmTraitFuncTranslation.h"
#include "Infra/Expected.h"
#include "Syntax/Syntaxes.g.h"
#include "RSymbol/RTraitDecl.h"
#include "RSymbol/RTraitFuncDecl.h"
#include "RSymbol/RTraitMemberDecl.h"
#include "RSymbol/RDeclKey.h"
#include "SmPhase4Context.h"
#include "SmPhaseManager.h"
#include "CommonTranslation.h"
#include "SmTypeTranslation.h"
#include "SmTypeResolveScope.h"
#include "ISmPhaseTask.h"
#include "ISmPhase4Task.h"

using namespace std;

namespace Citron {

using SmDeclContextPtr = shared_ptr<class SmDeclContext>;

namespace {

class SmTraitFuncTask
    : public ISmPhaseTask
    , public ISmPhase4Task
{
    SmDeclContextPtr traitDeclContext;
    RTraitDecl* rTraitDecl;
    STraitFuncDecl* sTraitFuncDecl;

    RFactoryPtr rFactory;

public:
    SmTraitFuncTask(InRefPtr<SmDeclContext> traitDeclContext, RTraitDecl* rTraitDecl, STraitFuncDecl* sTraitFuncDecl, InRefPtr<RFactory> rFactory);

public: // from ISmPhase4Task
    std::expected<void, DiagPtr> RunPhase4(SmPhase4Context& context) override;
};

} // namespace
} // namespace Citron

namespace Citron {

void SmTranslateTraitFunc(InRefPtr<SmDeclContext> traitDeclContext, RTraitDecl* rTraitDecl, STraitFuncDecl* sTraitFuncDecl, InRefPtr<RFactory> rFactory, SmPhaseManager& phaseManager)
{
    phaseManager.AddTask<SmTraitFuncTask>(traitDeclContext, rTraitDecl, sTraitFuncDecl, rFactory);
}

namespace {

SmTraitFuncTask::SmTraitFuncTask(InRefPtr<SmDeclContext> traitDeclContext, RTraitDecl* rTraitDecl, STraitFuncDecl* sTraitFuncDecl, InRefPtr<RFactory> rFactory)
    : traitDeclContext{traitDeclContext.Share()}, rTraitDecl{rTraitDecl}, sTraitFuncDecl{sTraitFuncDecl}, rFactory{rFactory.Share()}
{
}

expected<void, DiagPtr> SmTraitFuncTask::RunPhase4(SmPhase4Context& context)
{
    auto* rTraitFuncDecl = context.MakeRDecl<RTraitFuncDecl>(rTraitDecl, sTraitFuncDecl->bStatic, RName::Normal(sTraitFuncDecl->name));
    auto typeParams = MakeTypeParams(rTraitDecl->GetAllTypeParamCount(), rTraitFuncDecl, sTraitFuncDecl->typeParams, rFactory);

    SmTypeResolveScope_DeclHeader scope{traitDeclContext.get(), typeParams};
    auto e_funcRet = context.MakeFuncReturn(sTraitFuncDecl->funcRet, rTraitFuncDecl, typeParams, scope);
    RETURN_ON_ERROR(e_funcRet);

    auto e_paramResult = context.MakeFuncParameters(sTraitFuncDecl->parameters, scope);
    RETURN_ON_ERROR_REFDECL(e_paramResult, [rParameters, bLastParamVariadic]);

    rTraitFuncDecl->Init(RDeclKey::Func(RName::Normal(sTraitFuncDecl->name), rParameters), move(typeParams), move(*e_funcRet), move(rParameters), bLastParamVariadic);
    rTraitDecl->AddMember(RTraitMemberDecl{rTraitFuncDecl});
    return {};
}

} // namespace
} // namespace Citron