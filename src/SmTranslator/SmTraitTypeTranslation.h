#pragma once
#include <memory>
#include "Infra/Ref.h"

namespace Citron {

class STraitTypeDecl;
class RTraitDecl;
using RFactoryPtr = std::shared_ptr<class RFactory>;
class SmPhaseManager;
using SmDeclContextPtr = std::shared_ptr<class SmDeclContext>;

void SmTranslateTraitType(InRef<SmDeclContextPtr> traitDeclContext, STraitTypeDecl* sTraitTypeDecl, RTraitDecl* rTraitDecl, InRef<RFactoryPtr> rFactory, SmPhaseManager& phaseManager);

} // namespace Citron
