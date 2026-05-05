#include "StructFuncTask.h"

#include "Infra/Expected.h"

#include "NSymbol/NStructDecl.h"
#include "NSymbol/NStructFuncDecl.h"
#include "NSymbol/NFactory.h"

#include "MIR/MFuncBody.h"

#include "BuildTypeDependentSymbolContext.h"
#include "TranslateBodyContext.h"
#include "CommonTranslation.h"
#include "PhaseManager.h"

using namespace std;

namespace Citron {

void StructFuncTask::Register(NStructDecl* nStructDecl, SStructFuncDecl* syntax, const RFactoryPtr& rFactory, const NFactoryPtr& nFactory, PhaseManager& phaseManager)
{
    shared_ptr<StructFuncTask> task{new StructFuncTask(nStructDecl, syntax, rFactory, nFactory)};
    phaseManager.AddBuildTypeDependentSymbolTask(task);
    phaseManager.AddTranslateBodyTask(task);
}

expected<void, DiagPtr> StructFuncTask::BuildTypeDependentSymbol(BuildTypeDependentSymbolContext& context)
{
    auto accessor = MakeAccessor(sStruct->accessModifier, AccessorContext::InsideStruct);
    nStructFunc = nFactory->MakeNDecl<NStructFuncDecl>(
        nStruct, accessor, sStruct->bStatic, sStruct->bSequence,
        sStruct->name);

    auto typeParams = MakeTypeParams(nStructFunc, sStruct->typeParams, rFactory, *nFactory);
    nStructFunc->InitTypeParams(move(typeParams));

    nStruct->AddFunc(nStructFunc);

    // symbol tree에 매달린 nStructFunc가 필요
    auto* rRetType = context.MakeType(sStruct->retType, nStructFunc);
    
    auto e_parameters = context.MakeParameters(nStructFunc, sStruct->parameters);
    RETURN_ON_ERROR_REFDECL(e_parameters, [parameters, bLastParamVariadic]);

    nStructFunc->InitFuncReturnAndParams(rRetType, move(parameters), bLastParamVariadic);
    return {};
}

expected<MFuncBody, DiagPtr> StructFuncTask::TranslateBody(TranslateBodyContext& context)
{
    return context.Translate(nStructFunc, sStruct->body);
}


}