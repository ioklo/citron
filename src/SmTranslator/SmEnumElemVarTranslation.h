#pragma once
#include "Infra/RefPtr.h"

namespace Citron {

class SEnumElemVarDecl;
class REnumElemVarDecl;

class SmPhaseManager;
enum class AccessorContext;
class RFactory;
class SmDeclContext;
class REnumElemDecl;

void SmTranslateEnumElemVar(InRefPtr<SmDeclContext> enumElemDeclContext, SEnumElemVarDecl* sEnumElemVar, REnumElemDecl* rEnumElem, InRefPtr<RFactory> rFactory, SmPhaseManager& phaseManager);

} // namespace Citron

