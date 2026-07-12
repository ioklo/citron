#pragma once
#include "TranslationTasks.h"

namespace Citron {

class RTraitDecl;
class STraitFuncDecl;
class PhaseManager;

class TraitFuncTask
    : public IBuildTypeDependentSymbolTask
{
    RTraitDecl* rTraitDecl;
    STraitFuncDecl* sTraitFuncDecl;

public:
    static void Register(RTraitDecl* rTraitDecl, STraitFuncDecl* sTraitFuncDecl, PhaseManager& phaseManager);

private:
    TraitFuncTask(RTraitDecl* rTraitDecl, STraitFuncDecl* sTraitFuncDecl);

public: // from IBuildTypeDependentSymbolTask
    std::expected<void, DiagPtr> BuildTypeDependentSymbol(BuildTypeDependentSymbolContext& context) override;

};


} // namespace Citron
