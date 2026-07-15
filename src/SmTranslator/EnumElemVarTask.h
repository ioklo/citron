#pragma once

#include "TranslationTasks.h"

namespace Citron {

class SEnumDecl;
class SEnumElemDecl;
class SEnumElemVarDecl;

class REnumDecl;
class REnumElemDecl;
class REnumElemVarDecl;

class PhaseManager;
enum class AccessorContext;

class EnumElemVarTask
    : public IBuildNonTypeSymbolTask
{
    REnumElemVarDecl* rEnumElemVar;
    SEnumElemVarDecl* sEnumElemVar;
    EnumElemVarTask(REnumElemVarDecl* rEnumElemVar, SEnumElemVarDecl* sEnumElemVar)
        : rEnumElemVar{rEnumElemVar}, sEnumElemVar{sEnumElemVar}
    {
    }

public:
    static void Register(REnumElemVarDecl* rEnumElemVar, SEnumElemVarDecl* sEnumElemVar, PhaseManager& phaseManager);
    std::expected<void, DiagPtr> BuildNonTypeSymbol(BuildNonTypeSymbolContext& context) override;
};

} // namespace Citron

