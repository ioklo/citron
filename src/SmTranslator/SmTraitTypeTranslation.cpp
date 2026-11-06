#include "SmTraitTypeTranslation.h"
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
namespace {

class SmTraitTask : public IPostBuildTypeHierarchyTask
{
    SmDeclContextPtr traitDeclContext;
    STraitTypeDecl* sTraitTypeDecl;
    RTraitTypeDecl* rTraitTypeDecl;

public:
    SmTraitTask(InRef<SmDeclContextPtr> traitDeclContext, STraitTypeDecl* sTraitTypeDecl, RTraitTypeDecl* rTraitTypeDecl);

public: // from IPostBuildTypeHierarchyTask
    std::expected<void, DiagPtr> OnPostBuildTypeHierarchy(PostBuildTypeHierarchyContexts& contexts) override;
};


} // namespace
} // namespace Citron

namespace Citron {

void SmTranslateTraitType(InRef<SmDeclContextPtr> traitDeclContext, STraitTypeDecl* sTraitTypeDecl, RTraitDecl* rTraitDecl, InRef<RFactoryPtr> rFactory, SmPhaseManager& phaseManager)
{
    RName name = RName::Normal(sTraitTypeDecl->name);
    auto* rTraitTypeDecl = (*rFactory)->MakeDecl<RTraitTypeDecl>(rTraitDecl, RDeclKey::Normal(name), move(name));

    shared_ptr<SmTraitTask> task{new SmTraitTask{traitDeclContext, sTraitTypeDecl, rTraitTypeDecl}};
    phaseManager.AddPostBuildTypeHierarchyTask(task);
}

SmTraitTask::SmTraitTask(InRef<SmDeclContextPtr> traitDeclContext, STraitTypeDecl* sTraitTypeDecl, RTraitTypeDecl* rTraitTypeDecl)
    : traitDeclContext{*traitDeclContext}, sTraitTypeDecl{sTraitTypeDecl}, rTraitTypeDecl{rTraitTypeDecl}
{
}

expected<void, DiagPtr> SmTraitTask::OnPostBuildTypeHierarchy(PostBuildTypeHierarchyContexts& contexts)
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