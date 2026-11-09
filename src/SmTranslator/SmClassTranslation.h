#pragma once
#include "Infra/Ref.h"
#include <memory>

namespace Citron {

class SClassDecl;
class RTypeDeclOuter;
using RFactoryPtr = std::shared_ptr<class RFactory>;
class SmPhaseManager;
using SmDeclContextPtr = std::shared_ptr<class SmDeclContext>;

void SmTranslateClass(InRef<SmDeclContextPtr> outerDeclContext, RTypeDeclOuter outer, SClassDecl* syntax, InRef<RFactoryPtr> rFactory, SmPhaseManager& phaseManager);

} // namespace Citron