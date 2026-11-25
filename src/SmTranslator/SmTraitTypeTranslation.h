#pragma once
#include "Infra/RefPtr.h"

namespace Citron {

class STraitTypeDecl;
class RTraitDecl;
class RFactory;
class SmDeclContext;
class SmPhaseManager;

void SmTranslateTraitType(InRefPtr<SmDeclContext> traitDeclContext, STraitTypeDecl* sTraitTypeDecl, RTraitDecl* rTraitDecl, InRefPtr<RFactory> rFactory, SmPhaseManager& phaseManager);

} // namespace Citron
