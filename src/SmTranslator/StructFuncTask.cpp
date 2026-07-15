#include "StructFuncTask.h"

#include "Infra/Expected.h"
#include "RSymbol/RStructDecl.h"
#include "RSymbol/RStructFuncDecl.h"
#include "RSymbol/RFactory.h"

#include "MIR/MFuncBody.h"

#include "BuildNonTypeSymbolContext.h"
#include "TranslateBodyContext.h"
#include "CommonTranslation.h"
#include "PhaseManager.h"

using namespace std;

namespace Citron {

void StructFuncTask::Register(RStructDecl* rStructDecl, SStructFuncDecl* syntax, TakeRef<RFactoryPtr> rFactory, PhaseManager& phaseManager)
{
    shared_ptr<StructFuncTask> task{new StructFuncTask(rStructDecl, syntax, std::move(rFactory))};
    phaseManager.AddBuildNonTypeSymbolTask(task);
    phaseManager.AddTranslateBodyTask(task);
}

expected<void, DiagPtr> StructFuncTask::BuildNonTypeSymbol(BuildNonTypeSymbolContext& context)
{
    auto accessor = MakeStructMemberAccessor(sStructFunc->accessModifier);
    rStructFunc = rFactory->MakeDecl<RStructFuncDecl>(
        rStruct, accessor, RName::Normal(sStructFunc->name), sStructFunc->bSequence);

    auto typeParams = MakeTypeParams(rStructFunc, sStructFunc->typeParams, rFactory);
    rStructFunc->InitTypeParams(move(typeParams));

    rStruct->AddFunc(rStructFunc);

    // symbol tree에 매달린 rStructFunc가 필요
    auto e_funcRet = context.MakeFuncReturn(sStructFunc->funcRet, rStructFunc);
    RETURN_ON_ERROR(e_funcRet);

    auto e_parameters = context.MakeParameters(rStructFunc, sStructFunc->parameters);
    RETURN_ON_ERROR_REFDECL(e_parameters, [parameters, bLastParamVariadic]);

    rStructFunc->InitFuncReturnAndParams(sStructFunc->bStatic, move(*e_funcRet), move(parameters), bLastParamVariadic);
    return {};
}

expected<MFuncBody, DiagPtr> StructFuncTask::TranslateBody(TranslateBodyContext& context)
{
    return context.Translate(rStructFunc, sStructFunc->bSequence, sStructFunc->body);
}


}