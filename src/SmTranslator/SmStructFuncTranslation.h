#pragma once
#include <memory>
#include "Infra/Ref.h"
#include "TranslationTasks.h"

namespace Citron {

class RStructDecl;
class SStructFuncDecl;
using RFactoryPtr = std::shared_ptr<class RFactory>;
using SmDeclContextPtr = std::shared_ptr<class SmDeclContext>;

class SmPhaseManager;

void SmTranslateStructFunc(InRef<SmDeclContextPtr> structDeclContext, RStructDecl* rStructDecl, SStructFuncDecl* sStructDecl, InRef<RFactoryPtr> rFactory, SmPhaseManager& phaseManager);

} // namespace Citron