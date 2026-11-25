#pragma once
#include <memory>
#include "Infra/RefPtr.h"

namespace Citron {

class SScript;
class RModule;
class RFactory;
class SmPhaseManager;

void SmTranslateScript(SScript* sScript, RModule* rModule, InRefPtr<RFactory> rFactory, SmPhaseManager& phaseManager);

} // namespace Citron
