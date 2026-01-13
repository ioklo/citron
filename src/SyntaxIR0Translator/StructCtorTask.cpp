#include "StructCtorTask.h"

#include "Infra/Expected.h"
#include "NSymbol/NStructDecl.h"
#include "NSymbol/NStructCtorDecl.h"
#include "MIR/MFuncBody.h"

#include "CommonTranslation.h"
#include "BuildTypeDependentSymbolContext.h"
#include "TranslateBodyContext.h"
#include "PhaseManager.h"

using namespace std;

namespace Citron
{

void StructCtorTask::Register(NStructDecl* nStruct, SStructCtorDecl* sStructCtor, const NFactoryPtr& nFactory, PhaseManager& phaseManager)
{
    shared_ptr<StructCtorTask> task{new StructCtorTask{nStruct, sStructCtor, nFactory}};

    phaseManager.AddBuildTypeDependentSymbolTask(task);
    phaseManager.AddTranslateBodyTask(task);
}

expected<void, DiagPtr> StructCtorTask::BuildTypeDependentSymbol(BuildTypeDependentSymbolContext& context)
{
    auto accessor = MakeAccessor(sStructCtor->accessModifier, AccessorContext::InsideStruct);
    nStructCtor = nFactory->MakeNDecl<NStructCtorDecl>(nStruct, accessor, RStructCtorKind::Normal);
    nStruct->AddCtor(nStructCtor);

    // symbol tree에 매달린 nStructCtor가 필요
    auto e_parameters = context.MakeParameters(nStructCtor, sStructCtor->parameters);
    RETURN_ON_ERROR_REFDECL(e_parameters, [parameters, bLastParamVariadic]);

    nStructCtor->InitFuncParameters(move(parameters), bLastParamVariadic);
    return {};
}

expected<MFuncBody, DiagPtr> StructCtorTask::TranslateBody(TranslateBodyContext& context)
{
    return context.Translate(nStructCtor, sStructCtor->body);
}


} // namespace Citron
