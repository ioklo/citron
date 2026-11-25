#pragma once
#include <memory>
#include "Infra/RefPtr.h"

namespace Citron {

class SNamespaceDecl;
class SmDeclContext;

class RNamespace;
class RFactory;

class SmPhaseManager;

void SmTranslateNamespace(SNamespaceDecl* elem, InRefPtr<SmDeclContext> declContext, RNamespace* curNS, InRefPtr<RFactory> rFactory, SmPhaseManager& phaseManager);


} // namespace Citron
