#pragma once
#include "Infra/RefPtr.h"

namespace Citron {

class RStructDecl;
class SStructCtorDecl;
class SmPhaseManager;
class RFactory;
class SmDeclContext;

void SmTranslateStructCtor(InRefPtr<SmDeclContext> structDeclContext, RStructDecl* rStruct, SStructCtorDecl* sStructCtor, InRefPtr<RFactory> rFactory, SmPhaseManager& phaseManager);

} // namespace Citron