#include "SmImplTraitFuncTranslation.h"
#include "Infra/Ptr.h"
#include "Syntax/Syntax.h"
#include "RSymbol/RImplTraitFuncDecl.h"
#include "MIR/MFuncBody.h"
#include "SmDeclContext_Decl.h"
#include "SmPhase7Context.h"
#include "SmPhaseManager.h"
#include "ISmPhaseTask.h"
#include "ISmPhase7Task.h"

using namespace std;

namespace Citron {
namespace {

class SmImplTraitFuncTask
    : public ISmPhaseTask
    , public ISmPhase7Task
{
    SmDeclContextPtr outerDeclContext;
    RImplTraitFuncDecl* rImplTraitFuncDecl;
    SImplTraitFuncDecl* sImplTraitFuncDecl;
    RFactoryPtr rFactory;

public:
    SmImplTraitFuncTask(InRefPtr<SmDeclContext> outerDeclContext, RImplTraitFuncDecl* rImplTraitFuncDecl, SImplTraitFuncDecl* sImplTraitFuncDecl, InRefPtr<RFactory> rFactory);

    std::expected<MFuncBody, DiagPtr> RunPhase7(SmPhase7Context& context) final;
};

} // namespace
} // namespace Citron

namespace Citron {

void SmTranslateImplTraitFunc(InRefPtr<SmDeclContext> outerDeclContext, RImplTraitFuncDecl* rImplTraitFuncDecl, SImplTraitFuncDecl* sImplTraitFuncDecl, InRefPtr<RFactory> rFactory, SmPhaseManager& phaseManager)
{
    phaseManager.AddTask<SmImplTraitFuncTask>(outerDeclContext, rImplTraitFuncDecl, sImplTraitFuncDecl, rFactory);
}

namespace {

SmImplTraitFuncTask::SmImplTraitFuncTask(InRefPtr<SmDeclContext> outerDeclContext, RImplTraitFuncDecl* rImplTraitFuncDecl, SImplTraitFuncDecl* sImplTraitFuncDecl, InRefPtr<RFactory> rFactory)
    : outerDeclContext{outerDeclContext.Share()}, rImplTraitFuncDecl{rImplTraitFuncDecl}, sImplTraitFuncDecl{sImplTraitFuncDecl}, rFactory{rFactory.Share()}
{
}

expected<MFuncBody, DiagPtr> SmImplTraitFuncTask::RunPhase7(SmPhase7Context& context)
{
    SmDeclContextPtr declContext = MakePtr<SmDeclContext_Decl<RImplTraitFuncDecl>>(outerDeclContext, rImplTraitFuncDecl, rImplTraitFuncDecl->MakeOpenTypeArgs(*rFactory));
    return context.Translate(move(declContext), rImplTraitFuncDecl, sImplTraitFuncDecl->bSequence, sImplTraitFuncDecl->body);
}

} // namespace
} // namespace Citron