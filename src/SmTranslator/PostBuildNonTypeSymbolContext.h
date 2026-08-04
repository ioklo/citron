#pragma once
#include <memory>
#include <expected>
#include <span>
#include "Infra/Ref.h"
#include "RSymbol/RNames.h"
#include "RSymbol/RAppliedDecl.h"

namespace Citron {

using DiagPtr = std::shared_ptr<struct Diag>;
class STypeExp;
class RTraitDecl;
class RTypeArguments;
class RTypeParam;
class RTypeDecl;
class RFactory;
class SmDeclContext;
class PhaseManager;

class PostBuildNonTypeSymbolContext
{
    PhaseManager* phaseManager;
    RFactory* rFactory;

public:
    PostBuildNonTypeSymbolContext(PhaseManager* phaseManager, RFactory* rFactory) : phaseManager{phaseManager}, rFactory{rFactory} {}
    std::expected<RAppliedDecl<RTraitDecl>, DiagPtr> MakeTrait(STypeExp* sTypeExp, SmDeclContext* declContext, std::span<RTypeParam*> typeParams);
    PhaseManager* GetPhaseManager() { return phaseManager; }
};

} // namespace Citron
