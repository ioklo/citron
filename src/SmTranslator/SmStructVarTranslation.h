#pragma once
#include "Infra/RefPtr.h"

namespace Citron {

class RStructDecl;
class SStructVarDecl;

class SmPhaseManager;

class RFactory;
class SmDeclContext;

void SmTranslateStructVar(InRefPtr<SmDeclContext> structDeclContext, RStructDecl* rOuter, SStructVarDecl* syntax, InRefPtr<RFactory> rFactory, SmPhaseManager& phaseManager);

} // namespace Citron