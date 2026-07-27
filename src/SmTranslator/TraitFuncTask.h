#pragma once
#include "TranslationTasks.h"

namespace Citron {

class RTraitDecl;
class STraitFuncDecl;
class PhaseManager;

using RFactoryPtr = std::shared_ptr<class RFactory>;

class TraitFuncTask
    : public IBuildNonTypeSymbolTask
{
    RTraitDecl* rTraitDecl;
    STraitFuncDecl* sTraitFuncDecl;

    RFactoryPtr rFactory;

public:
    static void Register(RTraitDecl* rTraitDecl, STraitFuncDecl* sTraitFuncDecl, TakeRef<RFactoryPtr> rFactory, PhaseManager& phaseManager);

private:
    TraitFuncTask(RTraitDecl* rTraitDecl, STraitFuncDecl* sTraitFuncDecl, TakeRef<RFactoryPtr> rFactory);

public: // from IBuildNonTypeSymbolTask
    std::expected<void, DiagPtr> BuildNonTypeSymbol(BuildNonTypeSymbolContext& context) override;

};

} // namespace Citron
