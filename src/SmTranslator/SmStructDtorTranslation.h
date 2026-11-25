#pragma once
#include "Infra/RefPtr.h"

namespace Citron {

class RStructDecl;
class SStructDtorDecl;
class SmPhaseManager;
class RFactory;
class SmDeclContext;

void SmTranslateStructDtor(InRefPtr<SmDeclContext> structDeclContext, RStructDecl* rStruct, SStructDtorDecl* sStructDtor, InRefPtr<RFactory> rFactory, SmPhaseManager& phaseManager);

} // namespace Citron