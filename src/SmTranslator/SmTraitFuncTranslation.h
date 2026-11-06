#pragma once
#include "TranslationTasks.h"
#include "Infra/Ref.h"

namespace Citron {

class RTraitDecl;
class STraitFuncDecl;
class SmPhaseManager;

using RFactoryPtr = std::shared_ptr<class RFactory>;
using SmDeclContextPtr = std::shared_ptr<class SmDeclContext>;

void SmTranslateTraitFunc(InRef<SmDeclContextPtr> traitDeclContext, RTraitDecl* rTraitDecl, STraitFuncDecl* sTraitFuncDecl, InRef<RFactoryPtr> rFactory, SmPhaseManager& phaseManager);

} // namespace Citron
