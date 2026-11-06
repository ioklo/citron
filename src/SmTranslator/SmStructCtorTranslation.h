#pragma once
#include <memory>
#include "Infra/Ref.h"

namespace Citron {

class RStructDecl;
class SStructCtorDecl;
class SmPhaseManager;

using RFactoryPtr = std::shared_ptr<class RFactory>;
using SmDeclContextPtr = std::shared_ptr<class SmDeclContext>;

void SmTranslateStructCtor(InRef<SmDeclContextPtr> structDeclContext, RStructDecl* rStruct, SStructCtorDecl* sStructCtor, InRef<RFactoryPtr> rFactory, SmPhaseManager& phaseManager);

} // namespace Citron