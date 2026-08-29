#include "SmTraitTypeTask.h"
#include "Infra/Ptr.h"
#include "Infra/Expected.h"
#include "Syntax/Syntax.h"
#include "RSymbol/RDeclKey.h"
#include "RSymbol/RFactory.h"
#include "RSymbol/RTraitTypeDecl.h"
#include "SmPhaseManager.h"
#include "PostBuildTypeHierarchyContexts.h"

using namespace std;

namespace Citron {

void SmTraitTypeTask::BuildTypeHierarchy(SmDeclContextPtr& traitDeclContext, STraitTypeDecl* sTraitTypeDecl, RTraitDecl* rTraitDecl,  RFactory* rFactory, SmPhaseManager& phaseManager)
{
    RName name = RName::Normal(sTraitTypeDecl->name);
    auto* rTraitTypeDecl = rFactory->MakeDecl<RTraitTypeDecl>(rTraitDecl, RDeclKey::Normal(name), move(name));

    shared_ptr<SmTraitTypeTask> task{new SmTraitTypeTask{traitDeclContext, sTraitTypeDecl, rTraitTypeDecl}};
    phaseManager.AddPostBuildTypeHierarchyTask(task);
}

SmTraitTypeTask::SmTraitTypeTask(SmDeclContextPtr& traitDeclContext, STraitTypeDecl* sTraitTypeDecl, RTraitTypeDecl* rTraitTypeDecl)
    : traitDeclContext{traitDeclContext}, sTraitTypeDecl{sTraitTypeDecl}, rTraitTypeDecl{rTraitTypeDecl}
{
}

expected<void, DiagPtr> SmTraitTypeTask::OnPostBuildTypeHierarchy(PostBuildTypeHierarchyContexts& contexts)
{
    // traits를 채워 넣어야 한다
    vector<RAppliedDecl<RTraitDecl>> rTraits;
    for (auto* sTrait : sTraitTypeDecl->traits)
    {
        // TODO: [78] 2026-08-29, trait type requirements에 type parameter가 들어올 수 있게
        auto e_trait = contexts.MakeTrait(sTrait, traitDeclContext.get(), /*typeParams*/{});
        RETURN_ON_ERROR(e_trait);

        rTraits.push_back(*e_trait);
    }

    rTraitTypeDecl->Init(move(rTraits));
    return {};
}

} // namespace Citron