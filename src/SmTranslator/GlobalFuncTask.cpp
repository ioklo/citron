#include "GlobalFuncTask.h"

#include "Infra/Expected.h"
#include "Infra/Ptr.h"
#include "Syntax/Syntax.h"
#include "RSymbol/RGlobalFuncDecl.h"
#include "RSymbol/RNamespace.h"
#include "RSymbol/RFactory.h"

#include "MIR/MFuncBody.h"

#include "CommonTranslation.h"
#include "PhaseManager.h"
#include "BuildNonTypeSymbolContext.h"
#include "TranslateBodyContext.h"
#include "SmDeclContext_Decl.h"
#include "SmTypeTranslation.h"

using namespace std;

namespace Citron {

void GlobalFuncTask::Register(TakeRef<SmDeclContextPtr> outerDeclContext, RNamespace* outer, SGlobalFuncDecl* syntax, TakeRef<RFactoryPtr> rFactory, PhaseManager& phaseManager)
{
    shared_ptr<GlobalFuncTask> task{new GlobalFuncTask{move(outerDeclContext), outer, syntax, move(rFactory)}};
    phaseManager.AddBuildNonTypeSymbolTask(task);
    phaseManager.AddTranslateBodyTask(task);
}

expected<void, DiagPtr> GlobalFuncTask::BuildNonTypeSymbol(BuildNonTypeSymbolContext& context)
{
    auto accessor = MakeNamespaceMemberAccessor(syntax->accessModifier);
    bool bSeqFunc = false; // TODO:
    rFuncDecl = rFactory->MakeDecl<RGlobalFuncDecl>(
        rOuter, accessor, RName::Normal(syntax->name), bSeqFunc);

    auto typeParams = MakeTypeParams(rOuter->GetAllTypeParamCount(), rFuncDecl, syntax->typeParams, rFactory);

    SmTypeResolveScope_DeclHeader scope{outerDeclContext.get(), typeParams};
    auto e_funcRet = context.MakeFuncReturn(syntax->funcRet, scope);
    RETURN_ON_ERROR(e_funcRet);
    
    auto e_parametersInfo = context.MakeParameters(syntax->parameters, scope);
    RETURN_ON_ERROR_REFDECL(e_parametersInfo, [rParameters, bLastParamVariadic]);

    rFuncDecl->Init(RDeclKey::Func(RName::Normal(syntax->name), rParameters), move(typeParams), move(*e_funcRet), move(rParameters), bLastParamVariadic);
    rOuter->AddGlobalFuncDecl(rFuncDecl);

    return {};
}

expected<MFuncBody, DiagPtr> GlobalFuncTask::TranslateBody(TranslateBodyContext& context)
{
    // rFuncDecl에 대한 SmDeclContext를 만든다
    SmDeclContextPtr declContext = MakePtr<SmDeclContext_Decl<RGlobalFuncDecl>>(outerDeclContext, rFuncDecl, rFuncDecl->MakeOpenTypeArgs(*rFactory));
    return context.Translate(move(declContext), rFuncDecl, syntax->bSequence, syntax->body);
}

} // namespace Citron