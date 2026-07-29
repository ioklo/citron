#pragma once
#include "TranslationTasks.h"
#include "Infra/Ref.h"

namespace Citron {

class RTraitDecl;
class STraitFuncDecl;
class PhaseManager;

using RFactoryPtr = std::shared_ptr<class RFactory>;
using SmDeclContextPtr = std::shared_ptr<class SmDeclContext>;

class TraitFuncTask
    : public IBuildNonTypeSymbolTask
{
    SmDeclContextPtr traitDeclContext;
    RTraitDecl* rTraitDecl;
    STraitFuncDecl* sTraitFuncDecl;

    RFactoryPtr rFactory;

public:
    static void Register(TakeRef<SmDeclContextPtr> traitDeclContext, RTraitDecl* rTraitDecl, STraitFuncDecl* sTraitFuncDecl, TakeRef<RFactoryPtr> rFactory, PhaseManager& phaseManager);

private:
    TraitFuncTask(TakeRef<SmDeclContextPtr> traitDeclContext, RTraitDecl* rTraitDecl, STraitFuncDecl* sTraitFuncDecl, TakeRef<RFactoryPtr> rFactory);

public: // from IBuildNonTypeSymbolTask
    std::expected<void, DiagPtr> BuildNonTypeSymbol(BuildNonTypeSymbolContext& context) override;

};

} // namespace Citron
