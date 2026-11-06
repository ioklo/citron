#pragma once
#include <memory>
#include "Infra/Ref.h"

namespace Citron {

class STraitDecl;
class RTypeDeclOuter;
using RFactoryPtr = std::shared_ptr<class RFactory>;
class SmPhaseManager;
using SmDeclContextPtr = std::shared_ptr<class SmDeclContext>;

void SmTranslateTrait(InRef<SmDeclContextPtr> outerDeclContext, RTypeDeclOuter outer, STraitDecl* sTrait, InRef<RFactoryPtr> rFactory, SmPhaseManager& phaseManager);

} // namespace Citron