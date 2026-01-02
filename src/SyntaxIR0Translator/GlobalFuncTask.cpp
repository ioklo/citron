#include "GlobalFuncTask.h"

#include "Syntax/Syntax.h"
#include "NSymbol/NGlobalFuncDecl.h"
#include "NSymbol/NNamespaceDecl.h"

#include "MIR/MFuncBody.h"

#include "CommonTranslation.h"
#include "PhaseManager.h"
#include "BuildTypeDependentSymbolContext.h"
#include "TranslateBodyContext.h"

using namespace std;

namespace Citron {

void GlobalFuncTask::Register(NNamespaceDecl* outer, SGlobalFuncDecl* syntax, const NFactoryPtr& nFactory, PhaseManager& phaseManager)
{
    shared_ptr<GlobalFuncTask> task{new GlobalFuncTask(outer, syntax, nFactory)};
    phaseManager.AddBuildTypeDependentSymbolTask(task);
    phaseManager.AddTranslateBodyTask(task);
}

void GlobalFuncTask::BuildTypeDependentSymbol(BuildTypeDependentSymbolContext& context)
{
    auto accessor = MakeAccessor(syntax->accessModifier, AccessorContext::Global);    
    bool bSeqFunc = false; // TODO:
    nGFuncDecl = nFactory->MakeNDecl<NGlobalFuncDecl>(
        nOuter, accessor, bSeqFunc, RName_Normal(syntax->name));

    auto typeParams = MakeTypeParams(nGFuncDecl, syntax->typeParams, *nFactory);
    nGFuncDecl->InitTypeParams(move(typeParams));
    
    auto* rRetType = context.MakeType(syntax->retType, nGFuncDecl);
    auto [rParameters, bLastParamVariadic] = context.MakeParameters(nGFuncDecl, syntax->parameters);

    nGFuncDecl->InitFuncReturnAndParams(RFuncReturn_Set(rRetType), move(rParameters), bLastParamVariadic);
    nOuter->AddGlobalFuncDecl(nGFuncDecl);
}

expected<MFuncBody, DiagPtr> GlobalFuncTask::TranslateBody(TranslateBodyContext& context)
{
    return context.Translate(nGFuncDecl, syntax->body);
}

} // namespace Citron