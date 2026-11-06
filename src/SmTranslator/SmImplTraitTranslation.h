#pragma once
#include <memory>
#include "Infra/Ref.h"

namespace Citron {

class SImplTraitDecl;
class RImplTraitDeclOuter;
using RFactoryPtr = std::shared_ptr<class RFactory>; 

class SmPhaseManager;
using SmDeclContextPtr = std::shared_ptr<class SmDeclContext>;

void SmTranslateImplTrait(SImplTraitDecl* sImplDecl, InRef<SmDeclContextPtr> outerDeclContext, RImplTraitDeclOuter rOuter, InRef<RFactoryPtr> rFactory, SmPhaseManager& phaseManager);
} // namespace Citron
