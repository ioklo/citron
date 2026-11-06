#pragma once
#include <memory>
#include "Infra/Ref.h"

namespace Citron {

class SNamespaceDecl;
using SmDeclContextPtr = std::shared_ptr<class SmDeclContext>;

class RNamespace;
using RFactoryPtr = std::shared_ptr<class RFactory>;

class SmPhaseManager;

void SmTranslateNamespace(SNamespaceDecl* elem, InRef<SmDeclContextPtr> declContext, RNamespace* curNS, InRef<RFactoryPtr> rFactory, SmPhaseManager& phaseManager);


} // namespace Citron
