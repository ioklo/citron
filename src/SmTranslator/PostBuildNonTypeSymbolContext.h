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
class RTypeParam;
class RFactory;
class SmDeclContext;
class SmPhaseManager;
class SmFactory;

struct PostBuildNonTypeSymbolContexts
{
    SmPhaseManager* phaseManager;
    RFactory* rFactory;
    SmFactory* smFactory;
    
public:
    std::expected<RAppliedDecl<RTraitDecl>, DiagPtr> MakeTrait(STypeExp* sTypeExp, SmDeclContext* declContext, std::span<RTypeParam*> typeParams);
};

} // namespace Citron
