#pragma once
#include "Infra/RefPtr.h"

namespace Citron {

class SStructDecl;
class RTypeDeclOuter;
class RFactory;
class SmPhaseManager;
class SmDeclContext;

void SmTranslateStruct(InRefPtr<SmDeclContext> outerDeclContext, RTypeDeclOuter outer, SStructDecl* syntax, InRefPtr<RFactory> rFactory, SmPhaseManager& phaseManager);


} // namespace Citron