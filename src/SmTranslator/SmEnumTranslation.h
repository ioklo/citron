#pragma once
#include <memory>
#include "Infra/Ref.h"

namespace Citron {

class SEnumDecl;
class RTypeDeclOuter;
using RFactoryPtr = std::shared_ptr<class RFactory>;
class SmPhaseManager;
using SmDeclContextPtr = std::shared_ptr<class SmDeclContext>;

void SmTranslateEnum(InRef<SmDeclContextPtr> outerDeclContext, RTypeDeclOuter outer, SEnumDecl* sEnum, InRef<RFactoryPtr> rFactory, SmPhaseManager& phaseManager);

} // namespace Citron