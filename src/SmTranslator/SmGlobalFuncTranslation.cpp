#include "SmGlobalFuncTranslation.h"

#include "Infra/Expected.h"
#include "Infra/Ptr.h"
#include "Syntax/Syntax.h"
#include "RSymbol/RGlobalFuncDecl.h"
#include "RSymbol/RNamespace.h"
#include "RSymbol/RFactory.h"

#include "MIR/MFuncBody.h"

#include "CommonTranslation.h"
#include "SmPhaseManager.h"
#include "BuildNonTypeSymbolContext.h"
#include "TranslateBodyContext.h"
#include "SmDeclContext_Decl.h"
#include "SmTypeResolveScope.h"

using namespace std;

namespace Citron {
namespace {

class SmGlobalFuncTask
    : public IBuildNonTypeSymbolTask
    , public ITranslateBodyTask
{
    SmDeclContextPtr outerDeclContext;
    SGlobalFuncDecl* syntax;
    RNamespace* rOuter;
    RFactoryPtr rFactory;

    RGlobalFuncDecl* rFuncDecl;

public:
    SmGlobalFuncTask(InRef<SmDeclContextPtr> outerDeclContext, SGlobalFuncDecl* syntax, RNamespace* rOuter, InRef<RFactoryPtr> rFactory)
        : outerDeclContext{*outerDeclContext}, syntax{syntax}, rOuter{rOuter}, rFactory{*rFactory}, rFuncDecl{nullptr}
    {
    }

    std::expected<void, DiagPtr> BuildNonTypeSymbol(BuildNonTypeSymbolContext& context) override;
    std::expected<MFuncBody, DiagPtr> TranslateBody(TranslateBodyContext& context) override;
};

} // namespace
} // namespace Citron

namespace Citron {

void SmTranslateGlobalFunc(InRef<SmDeclContextPtr> outerDeclContext, SGlobalFuncDecl* syntax, RNamespace* outer, InRef<RFactoryPtr> rFactory, SmPhaseManager& phaseManager)
{
    shared_ptr<SmGlobalFuncTask> task{new SmGlobalFuncTask{move(outerDeclContext), syntax, outer, rFactory}};
    phaseManager.AddBuildNonTypeSymbolTask(task);
    phaseManager.AddTranslateBodyTask(task);
}

namespace {

expected<void, DiagPtr> SmGlobalFuncTask::BuildNonTypeSymbol(BuildNonTypeSymbolContext& context)
{
    auto accessor = MakeNamespaceMemberAccessor(syntax->accessModifier);
    bool bSeqFunc = false; // TODO:
    rFuncDecl = rFactory->MakeDecl<RGlobalFuncDecl>(
        rOuter, accessor, RName::Normal(syntax->name), bSeqFunc);

    auto typeParams = MakeTypeParams(rOuter->GetAllTypeParamCount(), rFuncDecl, syntax->typeParams, rFactory);

    SmTypeResolveScope_DeclHeader scope{outerDeclContext.get(), typeParams};
    auto e_funcRet = context.MakeFuncReturn(syntax->funcRet, rFuncDecl, typeParams, scope);
    RETURN_ON_ERROR(e_funcRet);

    auto e_parametersInfo = context.MakeFuncParameters(syntax->parameters, scope);
    RETURN_ON_ERROR_REFDECL(e_parametersInfo, [rParameters, bLastParamVariadic]);

    rFuncDecl->Init(RDeclKey::Func(RName::Normal(syntax->name), rParameters), move(typeParams), move(*e_funcRet), move(rParameters), bLastParamVariadic);
    rOuter->AddGlobalFuncDecl(rFuncDecl);

    return {};
}

expected<MFuncBody, DiagPtr> SmGlobalFuncTask::TranslateBody(TranslateBodyContext& context)
{
    // rFuncDecl에 대한 SmDeclContext를 만든다
    SmDeclContextPtr declContext = MakePtr<SmDeclContext_Decl<RGlobalFuncDecl>>(outerDeclContext, rFuncDecl, rFuncDecl->MakeOpenTypeArgs(*rFactory));
    return context.Translate(move(declContext), rFuncDecl, syntax->bSequence, syntax->body);
}

} // namespace
} // namespace Citron