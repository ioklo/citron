#pragma once
#include "Infra/RefPtr.h"

namespace Citron {

class STraitDecl;
class RTypeDeclOuter;
class RFactory;
class SmPhaseManager;
class SmDeclContext;

void SmTranslateTrait(InRefPtr<SmDeclContext> outerDeclContext, RTypeDeclOuter outer, STraitDecl* sTrait, InRefPtr<RFactory> rFactory, SmPhaseManager& phaseManager);

} // namespace Citron