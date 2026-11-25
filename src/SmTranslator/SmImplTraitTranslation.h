#pragma once
#include <memory>
#include "Infra/RefPtr.h"

namespace Citron {

class SImplTraitDecl;
class RImplTraitDeclOuter;
class RFactory;

class SmPhaseManager;
class SmDeclContext;

void SmTranslateImplTrait(SImplTraitDecl* sImplDecl, InRefPtr<SmDeclContext> outerDeclContext, RImplTraitDeclOuter rOuter, InRefPtr<RFactory> rFactory, SmPhaseManager& phaseManager);
} // namespace Citron
