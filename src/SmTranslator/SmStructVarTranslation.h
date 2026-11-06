#pragma once
#include <memory>
#include "Infra/Ref.h"

namespace Citron {

class RStructDecl;
class SStructVarDecl;

class SmPhaseManager;

using RFactoryPtr = std::shared_ptr<class RFactory>;
using SmDeclContextPtr = std::shared_ptr<class SmDeclContext>;

void SmTranslateStructVar(InRef<SmDeclContextPtr> structDeclContext, RStructDecl* rOuter, SStructVarDecl* syntax, InRef<RFactoryPtr> rFactory, SmPhaseManager& phaseManager);

} // namespace Citron