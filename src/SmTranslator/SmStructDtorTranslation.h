#pragma once
#include <memory>
#include "Infra/Ref.h"

namespace Citron {

class RStructDecl;
class SStructDtorDecl;
class SmPhaseManager;
using RFactoryPtr = std::shared_ptr<class RFactory>;
using SmDeclContextPtr = std::shared_ptr<class SmDeclContext>;

void SmTranslateStructDtor(InRef<SmDeclContextPtr> structDeclContext, RStructDecl* rStruct, SStructDtorDecl* sStructDtor, InRef<RFactoryPtr> rFactory, SmPhaseManager& phaseManager);

} // namespace Citron