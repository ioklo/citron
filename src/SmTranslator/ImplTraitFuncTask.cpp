#include "ImplTraitFuncTask.h"
#include "Infra/Ptr.h"
#include "Syntax/Syntax.h"
#include "RSymbol/RImplTraitFuncDecl.h"
#include "MIR/MFuncBody.h"
#include "SmDeclContext_Decl.h"
#include "TranslateBodyContext.h"
#include "SmPhaseManager.h"

using namespace std;

namespace Citron {

void ImplTraitFuncTask::Register(TakeRef<SmDeclContextPtr> outerDeclContext, RImplTraitFuncDecl* rImplTraitFuncDecl, SImplTraitFuncDecl* sImplTraitFuncDecl, TakeRef<RFactoryPtr> rFactory, SmPhaseManager& phaseManager)
{
    auto task = MakePtr<ImplTraitFuncTask>(move(outerDeclContext), rImplTraitFuncDecl, sImplTraitFuncDecl, move(rFactory));
    phaseManager.AddTranslateBodyTask(task);
}

ImplTraitFuncTask::ImplTraitFuncTask(TakeRef<SmDeclContextPtr> outerDeclContext, RImplTraitFuncDecl* rImplTraitFuncDecl, SImplTraitFuncDecl* sImplTraitFuncDecl, TakeRef<RFactoryPtr> rFactory)
    : outerDeclContext{outerDeclContext.Take()}, rImplTraitFuncDecl{rImplTraitFuncDecl}, sImplTraitFuncDecl{sImplTraitFuncDecl}, rFactory{rFactory.Take()}
{
}

expected<MFuncBody, DiagPtr> ImplTraitFuncTask::TranslateBody(TranslateBodyContext& context)
{
    SmDeclContextPtr declContext = MakePtr<SmDeclContext_Decl<RImplTraitFuncDecl>>(outerDeclContext, rImplTraitFuncDecl, rImplTraitFuncDecl->MakeOpenTypeArgs(*rFactory));
    return context.Translate(move(declContext), rImplTraitFuncDecl, sImplTraitFuncDecl->bSequence, sImplTraitFuncDecl->body);
}

} // namespace Citron