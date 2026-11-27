#pragma once
#include <expected>
#include <memory>
#include "Infra/RefPtr.h"

namespace Citron {

using DiagPtr = std::shared_ptr<struct Diag>;
class SImplTraitDecl;
class RImplTraitDeclOuter;
class RFactory;

class SmPhaseManager;
class SmDeclContext;

std::expected<void, DiagPtr> SmTranslateImplTrait(SImplTraitDecl* sImplDecl, InRefPtr<SmDeclContext> outerDeclContext, RImplTraitDeclOuter rOuter, InRefPtr<RFactory> rFactory, SmPhaseManager& phaseManager);
} // namespace Citron
