#include "SmImplTraitFuncTranslation.h"
#include "Infra/Ptr.h"
#include "Syntax/Syntax.h"
#include "RSymbol/RImplTraitFuncDecl.h"
#include "MIR/MFuncBody.h"
#include "SmDeclContext_Decl.h"
#include "TranslateBodyContext.h"
#include "SmPhaseManager.h"

using namespace std;

namespace Citron {
namespace {

class SmImplTraitFuncTask
    : public ITranslateBodyTask
{
    SmDeclContextPtr outerDeclContext;
    RImplTraitFuncDecl* rImplTraitFuncDecl;
    SImplTraitFuncDecl* sImplTraitFuncDecl;
    RFactoryPtr rFactory;

public:
    SmImplTraitFuncTask(InRef<SmDeclContextPtr> outerDeclContext, RImplTraitFuncDecl* rImplTraitFuncDecl, SImplTraitFuncDecl* sImplTraitFuncDecl, InRef<RFactoryPtr> rFactory);

    std::expected<MFuncBody, DiagPtr> TranslateBody(TranslateBodyContext& context) final;
};

} // namespace
} // namespace Citron

namespace Citron {

void SmTranslateImplTraitFunc(InRef<SmDeclContextPtr> outerDeclContext, RImplTraitFuncDecl* rImplTraitFuncDecl, SImplTraitFuncDecl* sImplTraitFuncDecl, InRef<RFactoryPtr> rFactory, SmPhaseManager& phaseManager)
{
    auto task = MakePtr<SmImplTraitFuncTask>(outerDeclContext, rImplTraitFuncDecl, sImplTraitFuncDecl, rFactory);
    phaseManager.AddTranslateBodyTask(task);
}

namespace {

SmImplTraitFuncTask::SmImplTraitFuncTask(InRef<SmDeclContextPtr> outerDeclContext, RImplTraitFuncDecl* rImplTraitFuncDecl, SImplTraitFuncDecl* sImplTraitFuncDecl, InRef<RFactoryPtr> rFactory)
    : outerDeclContext{*outerDeclContext}, rImplTraitFuncDecl{rImplTraitFuncDecl}, sImplTraitFuncDecl{sImplTraitFuncDecl}, rFactory{*rFactory}
{
}

expected<MFuncBody, DiagPtr> SmImplTraitFuncTask::TranslateBody(TranslateBodyContext& context)
{
    SmDeclContextPtr declContext = MakePtr<SmDeclContext_Decl<RImplTraitFuncDecl>>(outerDeclContext, rImplTraitFuncDecl, rImplTraitFuncDecl->MakeOpenTypeArgs(*rFactory));
    return context.Translate(move(declContext), rImplTraitFuncDecl, sImplTraitFuncDecl->bSequence, sImplTraitFuncDecl->body);
}

} // namespace
} // namespace Citron