#include "StructFuncTask.h"

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

void StructFuncTask::Register(NStructDecl* nStructDecl, SStructFuncDecl* syntax, const NFactoryPtr& nFactory, PhaseManager& phaseManager)
{
    shared_ptr<StructFuncTask> task{new StructFuncTask(nStructDecl, syntax, nFactory)};
    phaseManager.AddBuildTypeDependentSymbolTask(task);
    phaseManager.AddTranslateBodyTask(task);
}

void StructFuncTask::BuildTypeDependentSymbol(BuildTypeDependentSymbolContext& context)
{
    auto accessor = MakeAccessor(sStruct->accessModifier, AccessorContext::InsideStruct);
    auto typeParams = MakeTypeParams(sStruct->typeParams);
    nStructFunc = nFactory->MakeNDecl<NStructFuncDecl>(
        nStruct, accessor, sStruct->bStatic, sStruct->bSequence,
        sStruct->name, move(typeParams));
    nStruct->AddFunc(nStructFunc);

    // symbol tree에 매달린 nStructFunc가 필요
    auto* rRetType = context.MakeType(sStruct->retType, nStructFunc);
    auto [rParameters, bLastParamVariadic] = context.MakeParameters(nStructFunc, sStruct->parameters);
    nStructFunc->InitFuncReturnAndParams(rRetType, move(rParameters), bLastParamVariadic);
}

expected<MFuncBody, DiagPtr> StructFuncTask::TranslateBody(TranslateBodyContext& context)
{
    return context.Translate(nStructFunc, sStruct->body);
}


}