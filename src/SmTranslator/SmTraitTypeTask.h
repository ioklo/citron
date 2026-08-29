#pragma once
#include <memory>
#include <expected>
#include "TranslationTasks.h"

namespace Citron {

class STraitTypeDecl;
class RTraitDecl;
class RTraitTypeDecl;
class RFactory;
class SmPhaseManager;
using SmDeclContextPtr = std::shared_ptr<class SmDeclContext>;

class SmTraitTypeTask : public IPostBuildTypeHierarchyTask
{
    SmDeclContextPtr traitDeclContext;
    STraitTypeDecl* sTraitTypeDecl;
    RTraitTypeDecl* rTraitTypeDecl;

public:
    static void BuildTypeHierarchy(SmDeclContextPtr& traitDeclContext, STraitTypeDecl* sTraitTypeDecl, RTraitDecl* rTraitDecl, RFactory* rFactory, SmPhaseManager& phaseManager);

private:
    SmTraitTypeTask(SmDeclContextPtr& traitDeclContext, STraitTypeDecl* sTraitTypeDecl, RTraitTypeDecl* rTraitTypeDecl);

public: // from IPostBuildTypeHierarchyTask
    std::expected<void, DiagPtr> OnPostBuildTypeHierarchy(PostBuildTypeHierarchyContexts& contexts) override;
};

} // namespace Citron
