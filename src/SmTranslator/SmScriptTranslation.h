#pragma once
#include <memory>
#include "Infra/Ref.h"

namespace Citron {

class SScript;
class RModule;
using RFactoryPtr = std::shared_ptr<class RFactory>;
class SmPhaseManager;

void SmTranslateScript(SScript* sScript, RModule* rModule, InRef<RFactoryPtr> rFactory, SmPhaseManager& phaseManager);

} // namespace Citron
