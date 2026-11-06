#pragma once
#include "Infra/Ref.h"
#include "TranslationTasks.h"


namespace Citron {

class SEnumElemVarDecl;
class REnumElemVarDecl;

class SmPhaseManager;
enum class AccessorContext;
using RFactoryPtr = std::shared_ptr<class RFactory>;
using SmDeclContextPtr = std::shared_ptr<class SmDeclContext>;

void SmTranslateEnumElemVar(InRef<SmDeclContextPtr> enumElemDeclContext, SEnumElemVarDecl* sEnumElemVar, REnumElemDecl* rEnumElem, InRef<RFactoryPtr> rFactory, SmPhaseManager& phaseManager);

} // namespace Citron

