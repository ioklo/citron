#pragma once
#include "Infra/RefPtr.h"

namespace Citron {

class RStructDecl;
class SStructFuncDecl;
class RFactory;
class SmDeclContext;

class SmPhaseManager;

void SmTranslateStructFunc(InRefPtr<SmDeclContext> structDeclContext, RStructDecl* rStructDecl, SStructFuncDecl* sStructDecl, InRefPtr<RFactory> rFactory, SmPhaseManager& phaseManager);

} // namespace Citron