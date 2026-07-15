#pragma once
#include "TranslationTasks.h"

namespace Citron {

class RTraitDecl;
class STraitFuncDecl;
class PhaseManager;

class TraitFuncTask
    : public IBuildNonTypeSymbolTask
{
    RTraitDecl* rTraitDecl;
    STraitFuncDecl* sTraitFuncDecl;

public:
    static void Register(RTraitDecl* rTraitDecl, STraitFuncDecl* sTraitFuncDecl, PhaseManager& phaseManager);

private:
    TraitFuncTask(RTraitDecl* rTraitDecl, STraitFuncDecl* sTraitFuncDecl);

public: // from IBuildNonTypeSymbolTask
    std::expected<void, DiagPtr> BuildNonTypeSymbol(BuildNonTypeSymbolContext& context) override;

};

} // namespace Citron
