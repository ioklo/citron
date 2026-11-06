#pragma once
#include <memory>
#include "Infra/Ref.h"
#include "TranslationTasks.h"

namespace Citron {

class SStructDecl;
class RTypeDeclOuter;
using RFactoryPtr = std::shared_ptr<class RFactory>;
class SmPhaseManager;
using SmDeclContextPtr = std::shared_ptr<class SmDeclContext>;

void SmTranslateStruct(InRef<SmDeclContextPtr> outerDeclContext, RTypeDeclOuter outer, SStructDecl* syntax, InRef<RFactoryPtr> rFactory, SmPhaseManager& phaseManager);


} // namespace Citron