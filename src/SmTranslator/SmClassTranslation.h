#pragma once
#include "Infra/RefPtr.h"

namespace Citron {

class SClassDecl;
class RTypeDeclOuter;
class RFactory;
class SmPhaseManager;
class SmDeclContext;

void SmTranslateClass(InRefPtr<SmDeclContext> outerDeclContext, RTypeDeclOuter outer, SClassDecl* syntax, InRefPtr<RFactory> rFactory, SmPhaseManager& phaseManager);

} // namespace Citron