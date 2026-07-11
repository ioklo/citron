#include "GlobalFuncTask.h"

#include "Infra/Expected.h"
#include "Syntax/Syntax.h"
#include "RSymbol/RGlobalFuncDecl.h"
#include "RSymbol/RNamespaceDecl.h"
#include "RSymbol/RFactory.h"

#include "MIR/MFuncBody.h"

#include "CommonTranslation.h"
#include "PhaseManager.h"
#include "BuildTypeDependentSymbolContext.h"
#include "TranslateBodyContext.h"

using namespace std;

namespace Citron {

void GlobalFuncTask::Register(RNamespaceDecl* outer, SGlobalFuncDecl* syntax, TakeRef<RFactoryPtr> rFactory, PhaseManager& phaseManager)
{
    shared_ptr<GlobalFuncTask> task{new GlobalFuncTask(outer, syntax, move(rFactory))};
    phaseManager.AddBuildTypeDependentSymbolTask(task);
    phaseManager.AddTranslateBodyTask(task);
}

expected<void, DiagPtr> GlobalFuncTask::BuildTypeDependentSymbol(BuildTypeDependentSymbolContext& context)
{
    auto accessor = MakeNamespaceMemberAccessor(syntax->accessModifier);
    bool bSeqFunc = false; // TODO:
    rFuncDecl = rFactory->MakeDecl<RGlobalFuncDecl>(
        rOuter, accessor, RName::Normal(syntax->name), bSeqFunc);

    auto typeParams = MakeTypeParams(rFuncDecl, syntax->typeParams, rFactory);
    rFuncDecl->InitTypeParams(move(typeParams));

    auto e_funcRet = context.MakeFuncReturn(syntax->funcRet, rFuncDecl);
    RETURN_ON_ERROR(e_funcRet);
    
    auto e_parametersInfo = context.MakeParameters(rFuncDecl, syntax->parameters);
    RETURN_ON_ERROR_REFDECL(e_parametersInfo, [rParameters, bLastParamVariadic]);

    rFuncDecl->InitFuncReturnAndParams(move(*e_funcRet), move(rParameters), bLastParamVariadic);
    rOuter->AddGlobalFuncDecl(rFuncDecl);

    return {};
}

expected<MFuncBody, DiagPtr> GlobalFuncTask::TranslateBody(TranslateBodyContext& context)
{
    return context.Translate(rFuncDecl, syntax->bSequence, syntax->body);
}

} // namespace Citron