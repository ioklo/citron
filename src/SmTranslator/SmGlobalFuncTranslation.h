#pragma once
#include "Infra/Ref.h"
#include "Infra/RefPtr.h"

namespace Citron {

class RNamespace;
class SGlobalFuncDecl;
class RGlobalFuncDecl;
class RFactory;
class SmDeclContext;
class SmPhaseManager;

void SmTranslateGlobalFunc(InRefPtr<SmDeclContext> outerDeclContext, SGlobalFuncDecl* sGFuncDecl, RNamespace* outer, InRefPtr<RFactory> rFactory, SmPhaseManager& phaseManager);


} // namespace Citron