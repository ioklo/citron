#include "TraitFuncTask.h"
#include "Infra/Expected.h"
#include "Syntax/Syntaxes.g.h"
#include "RSymbol/RTraitDecl.h"
#include "RSymbol/RTraitFuncDecl.h"
#include "RSymbol/RTraitMemberDecl.h"
#include "RSymbol/RDeclKey.h"
#include "BuildNonTypeSymbolContext.h"
#include "PhaseManager.h"
#include "CommonTranslation.h"

using namespace std;

namespace Citron {

void TraitFuncTask::Register(RTraitDecl* rTraitDecl, STraitFuncDecl* sTraitFuncDecl, TakeRef<RFactoryPtr> rFactory, PhaseManager& phaseManager)
{
    shared_ptr<TraitFuncTask> task{new TraitFuncTask{rTraitDecl, sTraitFuncDecl, move(rFactory)}};

    phaseManager.AddBuildNonTypeSymbolTask(task);
}

TraitFuncTask::TraitFuncTask(RTraitDecl* rTraitDecl, STraitFuncDecl* sTraitFuncDecl, TakeRef<RFactoryPtr> rFactory)
    : rTraitDecl{rTraitDecl}, sTraitFuncDecl{sTraitFuncDecl}, rFactory{rFactory.Take()}
{
}

expected<void, DiagPtr> TraitFuncTask::BuildNonTypeSymbol(BuildNonTypeSymbolContext& context)
{
    auto* rTraitFuncDecl = context.MakeRDecl<RTraitFuncDecl>(rTraitDecl, sTraitFuncDecl->bStatic, RName::Normal(sTraitFuncDecl->name));
    auto typeParams = MakeTypeParams(rTraitDecl->GetAllTypeParamCount(), rTraitFuncDecl, sTraitFuncDecl->typeParams, rFactory);

    SmFuncHeaderResolveScope scope{rTraitDecl, typeParams};
    auto e_funcRet = context.MakeFuncReturn(sTraitFuncDecl->funcRet, scope);
    RETURN_ON_ERROR(e_funcRet);

    auto e_paramResult = context.MakeParameters(sTraitFuncDecl->parameters, scope);
    RETURN_ON_ERROR_REFDECL(e_paramResult, [rParameters, bLastParamVariadic]);
    
    rTraitFuncDecl->Init(RDeclKey::Func(RName::Normal(sTraitFuncDecl->name), rParameters), move(typeParams), move(*e_funcRet), move(rParameters), bLastParamVariadic);
    rTraitDecl->AddMember(RTraitMemberDecl{rTraitFuncDecl});
    return {};
}

} // namespace Citron