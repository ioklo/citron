#pragma once
#include <memory>
#include "Infra/RefPtr.h"

namespace Citron {

class SEnumDecl;
class RTypeDeclOuter;
class RFactory;
class SmPhaseManager;
class SmDeclContext;

void SmTranslateEnum(InRefPtr<SmDeclContext> outerDeclContext, RTypeDeclOuter outer, SEnumDecl* sEnum, InRefPtr<RFactory> rFactory, SmPhaseManager& phaseManager);

} // namespace Citron