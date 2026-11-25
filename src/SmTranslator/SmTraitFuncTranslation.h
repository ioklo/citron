#pragma once
#include "Infra/RefPtr.h"

namespace Citron {

class RTraitDecl;
class STraitFuncDecl;
class SmPhaseManager;
class RFactory;
class SmDeclContext;

void SmTranslateTraitFunc(InRefPtr<SmDeclContext> traitDeclContext, RTraitDecl* rTraitDecl, STraitFuncDecl* sTraitFuncDecl, InRefPtr<RFactory> rFactory, SmPhaseManager& phaseManager);

} // namespace Citron
