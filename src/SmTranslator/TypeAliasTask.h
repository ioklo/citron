#pragma once

#include <memory>

#include "Infra/Ref.h"
#include "RSymbol/RTypeDeclOuter.h"
#include "TranslationTasks.h"

namespace Citron {

class STypeAliasDecl;
class RTypeAliasDecl;
class SmPhaseManager;
using RFactoryPtr = std::shared_ptr<class RFactory>;
using SmDeclContextPtr = std::shared_ptr<class SmDeclContext>;

class TypeAliasTask final : public IBuildTypeHierarchyTask
{
    SmDeclContextPtr outerDeclContext;
    STypeAliasDecl* sTypeAliasDecl;
    RTypeAliasDecl* rTypeAliasDecl;
    RFactoryPtr rFactory;

    TypeAliasTask(TakeRef<SmDeclContextPtr> outerDeclContext, STypeAliasDecl* sTypeAliasDecl, RTypeAliasDecl* rTypeAliasDecl, TakeRef<RFactoryPtr> rFactory);

public:
    static void Register(TakeRef<SmDeclContextPtr> outerDeclContext, RTypeDeclOuter outer, STypeAliasDecl* sTypeAliasDecl, TakeRef<RFactoryPtr> rFactory, SmPhaseManager& phaseManager);

public: // from IBuildTypeHierarchyTask
    std::expected<void, DiagPtr> BuildTypeHierarchy(BuildTypeHierarchyContext& context) final;
};

} // namespace Citron
