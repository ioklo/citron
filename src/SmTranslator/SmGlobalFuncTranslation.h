#pragma once
#include <memory>
#include <expected>

#include "Infra/Ref.h"
#include "TranslationTasks.h"

namespace Citron {

class RNamespace;
class SGlobalFuncDecl;
class RGlobalFuncDecl;
using RFactoryPtr = std::shared_ptr<class RFactory>;
using SmDeclContextPtr = std::shared_ptr<class SmDeclContext>;

class SmPhaseManager;

void SmTranslateGlobalFunc(InRef<SmDeclContextPtr> outerDeclContext, SGlobalFuncDecl* sGFuncDecl, RNamespace* outer, InRef<RFactoryPtr> rFactory, SmPhaseManager& phaseManager);


} // namespace Citron