#pragma once

#include "TranslationTasks.h"

namespace Citron {

class SEnumDecl;
class SEnumElemDecl;
class SEnumElemVarDecl;

class NEnumDecl;
class NEnumElemDecl;
class NEnumElemVarDecl;

class PhaseManager;
enum class AccessorContext;

class EnumElemVarTask
    : public IBuildTypeDependentSymbolTask
{
    NEnumElemVarDecl* nEnumElemVar;
    SEnumElemVarDecl* sEnumElemVar;
    EnumElemVarTask(NEnumElemVarDecl* nEnumElemVar, SEnumElemVarDecl* sEnumElemVar)
        : nEnumElemVar{nEnumElemVar}, sEnumElemVar{sEnumElemVar}
    {
    }

public:
    static void Register(NEnumElemVarDecl* nEnumElemVar, SEnumElemVarDecl* sEnumElemVar, PhaseManager& phaseManager);
    void BuildTypeDependentSymbol(BuildTypeDependentSymbolContext& context) override;
};

} // namespace Citron

