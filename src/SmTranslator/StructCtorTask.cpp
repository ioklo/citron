#include "StructCtorTask.h"

#include "Infra/Expected.h"
#include "RSymbol/RStructDecl.h"
#include "RSymbol/RStructCtorDecl.h"
#include "MIR/MFuncBody.h"

#include "CommonTranslation.h"
#include "BuildTypeDependentSymbolContext.h"
#include "TranslateBodyContext.h"
#include "PhaseManager.h"

using namespace std;

namespace Citron
{

void StructCtorTask::Register(RStructDecl* rStruct, SStructCtorDecl* sStructCtor, TakeRef<RFactoryPtr> rFactory, PhaseManager& phaseManager)
{
    shared_ptr<StructCtorTask> task{new StructCtorTask{rStruct, sStructCtor, std::move(rFactory)}};

    phaseManager.AddBuildTypeDependentSymbolTask(task);
    phaseManager.AddTranslateBodyTask(task);
}

expected<void, DiagPtr> StructCtorTask::BuildTypeDependentSymbol(BuildTypeDependentSymbolContext& context)
{
    auto accessor = MakeStructMemberAccessor(sStructCtor->accessModifier);
    rStructCtor = rFactory->MakeDecl<RStructCtorDecl>(rStruct, accessor, RStructCtorKind::Normal);
    rStruct->AddCtor(rStructCtor);

    // symbol tree에 매달린 rStructCtor가 필요
    auto e_parameters = context.MakeParameters(rStructCtor, sStructCtor->parameters);
    RETURN_ON_ERROR_REFDECL(e_parameters, [parameters, bLastParamVariadic]);

    rStructCtor->InitFuncParameters(move(parameters), bLastParamVariadic);
    return {};
}

expected<MFuncBody, DiagPtr> StructCtorTask::TranslateBody(TranslateBodyContext& context)
{
    return context.Translate(rStructCtor, sStructCtor->body);
}


} // namespace Citron
