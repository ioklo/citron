#include "TraitFuncTask.h"
#include "Infra/Expected.h"
#include "Syntax/Syntaxes.g.h"
#include "RSymbol/RTraitDecl.h"
#include "RSymbol/RTraitFuncDecl.h"
#include "RSymbol/RTraitMemberDecl.h"
#include "BuildTypeDependentSymbolContext.h"
#include "PhaseManager.h"

using namespace std;

namespace Citron {

void TraitFuncTask::Register(RTraitDecl* rTraitDecl, STraitFuncDecl* sTraitFuncDecl, PhaseManager& phaseManager)
{
    shared_ptr<TraitFuncTask> task{new TraitFuncTask{rTraitDecl, sTraitFuncDecl}};

    phaseManager.AddBuildTypeDependentSymbolTask(task);
}

TraitFuncTask::TraitFuncTask(RTraitDecl* rTraitDecl, STraitFuncDecl* sTraitFuncDecl)
    : rTraitDecl{rTraitDecl}, sTraitFuncDecl{sTraitFuncDecl}
{
}

expected<void, DiagPtr> TraitFuncTask::BuildTypeDependentSymbol(BuildTypeDependentSymbolContext& context)
{
    auto e_funcRet = context.MakeFuncReturn(sTraitFuncDecl->funcRet, rTraitDecl);
    RETURN_ON_ERROR(e_funcRet);

    auto e_paramResult = context.MakeParameters(rTraitDecl, sTraitFuncDecl->parameters);
    RETURN_ON_ERROR_REFDECL(e_paramResult, [rParameters, bLastParamVariadic]);

    auto* rTraitFuncDecl = context.MakeRDecl<RTraitFuncDecl>(rTraitDecl, sTraitFuncDecl->bStatic, move(*e_funcRet), RName::Normal(sTraitFuncDecl->name), move(rParameters), bLastParamVariadic);
    rTraitDecl->AddMember(RTraitMemberDecl{rTraitFuncDecl});

    return {};
}

} // namespace Citron