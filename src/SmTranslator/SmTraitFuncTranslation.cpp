#include "SmTraitFuncTranslation.h"
#include "Infra/Expected.h"
#include "Syntax/Syntaxes.g.h"
#include "RSymbol/RTraitDecl.h"
#include "RSymbol/RTraitFuncDecl.h"
#include "RSymbol/RTraitMemberDecl.h"
#include "RSymbol/RDeclKey.h"
#include "BuildNonTypeSymbolContext.h"
#include "SmPhaseManager.h"
#include "CommonTranslation.h"
#include "SmTypeTranslation.h"
#include "SmTypeResolveScope.h"

using namespace std;

namespace Citron {
namespace {

class SmTraitFuncTask
    : public IBuildNonTypeSymbolTask
{
    SmDeclContextPtr traitDeclContext;
    RTraitDecl* rTraitDecl;
    STraitFuncDecl* sTraitFuncDecl;

    RFactoryPtr rFactory;

public:
    SmTraitFuncTask(InRef<SmDeclContextPtr> traitDeclContext, RTraitDecl* rTraitDecl, STraitFuncDecl* sTraitFuncDecl, InRef<RFactoryPtr> rFactory);

public: // from IBuildNonTypeSymbolTask
    std::expected<void, DiagPtr> BuildNonTypeSymbol(BuildNonTypeSymbolContext& context) override;
};

} // namespace
} // namespace Citron

namespace Citron {

void SmTranslateTraitFunc(InRef<SmDeclContextPtr> traitDeclContext, RTraitDecl* rTraitDecl, STraitFuncDecl* sTraitFuncDecl, InRef<RFactoryPtr> rFactory, SmPhaseManager& phaseManager)
{
    shared_ptr<SmTraitFuncTask> task{new SmTraitFuncTask{move(traitDeclContext), rTraitDecl, sTraitFuncDecl, move(rFactory)}};
    phaseManager.AddBuildNonTypeSymbolTask(task);
}

namespace {

SmTraitFuncTask::SmTraitFuncTask(InRef<SmDeclContextPtr> traitDeclContext, RTraitDecl* rTraitDecl, STraitFuncDecl* sTraitFuncDecl, InRef<RFactoryPtr> rFactory)
    : traitDeclContext{*traitDeclContext}, rTraitDecl{rTraitDecl}, sTraitFuncDecl{sTraitFuncDecl}, rFactory{*rFactory}
{
}

expected<void, DiagPtr> SmTraitFuncTask::BuildNonTypeSymbol(BuildNonTypeSymbolContext& context)
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