#pragma once
#include "Infra/RefPtr.h"

namespace Citron {

class SImplTraitFuncDecl;

class RImplTraitFuncDecl;
class RFactory;

class SmPhaseManager;
class SmDeclContext;

void SmTranslateImplTraitFunc(InRefPtr<SmDeclContext> outerDeclContext, RImplTraitFuncDecl* rImplTraitFuncDecl, SImplTraitFuncDecl* sImplTraitFuncDecl, InRefPtr<RFactory> rFactory, SmPhaseManager& phaseManager);

} // namespace Citron
