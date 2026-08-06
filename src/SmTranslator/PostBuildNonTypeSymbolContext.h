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

struct PostBuildNonTypeSymbolContexts
{
    PhaseManager* phaseManager;
    RFactory* rFactory;
    
public:
    std::expected<RAppliedDecl<RTraitDecl>, DiagPtr> MakeTrait(STypeExp* sTypeExp, SmDeclContext* declContext, std::span<RTypeParam*> typeParams);
};

} // namespace Citron
