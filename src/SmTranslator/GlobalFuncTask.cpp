#include "GlobalFuncTask.h"

#include "Infra/Expected.h"
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

void GlobalFuncTask::Register(NNamespaceDecl* outer, SGlobalFuncDecl* syntax, const RFactoryPtr& rFactory, const NFactoryPtr& nFactory, PhaseManager& phaseManager)
{
    shared_ptr<GlobalFuncTask> task{new GlobalFuncTask(outer, syntax, rFactory, nFactory)};
    phaseManager.AddBuildTypeDependentSymbolTask(task);
    phaseManager.AddTranslateBodyTask(task);
}

expected<void, DiagPtr> GlobalFuncTask::BuildTypeDependentSymbol(BuildTypeDependentSymbolContext& context)
{
    auto accessor = MakeAccessor(syntax->accessModifier, AccessorContext::Global);    
    bool bSeqFunc = false; // TODO:
    nGFuncDecl = nFactory->MakeNDecl<NGlobalFuncDecl>(
        nOuter, accessor, bSeqFunc, RName_Normal(syntax->name));

    auto typeParams = MakeTypeParams(nGFuncDecl, syntax->typeParams, rFactory, *nFactory);
    nGFuncDecl->InitTypeParams(move(typeParams));

    auto e_funcRet = context.MakeFuncReturn(syntax->funcRet, nGFuncDecl);
    RETURN_ON_ERROR(e_funcRet);
    
    auto e_parametersInfo = context.MakeParameters(nGFuncDecl, syntax->parameters);
    RETURN_ON_ERROR_REFDECL(e_parametersInfo, [rParameters, bLastParamVariadic]);

    nGFuncDecl->InitFuncReturnAndParams(move(*e_funcRet), move(rParameters), bLastParamVariadic);
    nOuter->AddGlobalFuncDecl(nGFuncDecl);

    return {};
}

expected<MFuncBody, DiagPtr> GlobalFuncTask::TranslateBody(TranslateBodyContext& context)
{
    return context.Translate(nGFuncDecl, syntax->body);
}

} // namespace Citron