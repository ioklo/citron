#pragma once
#include <memory>
#include "Infra/Ref.h"

namespace Citron {

class SImplTraitFuncDecl;

class RImplTraitFuncDecl;
using RFactoryPtr = std::shared_ptr<class RFactory>;

class SmPhaseManager;
using SmDeclContextPtr = std::shared_ptr<class SmDeclContext>;

void SmTranslateImplTraitFunc(InRef<SmDeclContextPtr> outerDeclContext, RImplTraitFuncDecl* rImplTraitFuncDecl, SImplTraitFuncDecl* sImplTraitFuncDecl, InRef<RFactoryPtr> rFactory, SmPhaseManager& phaseManager);

} // namespace Citron
