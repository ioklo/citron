#pragma once

#include "TranslationTasks.h"

namespace Citron {

class SEnumDecl;
class SEnumElemDecl;
class SEnumElemVarDecl;

class NEnumDecl;
class NEnumElemDecl;
class NEnumElemVarDecl;

namespace SyntaxIR0Translator {

class PhaseManager;
enum class AccessorContext;

class EnumTask
    : public IBuildTypeSymbolTask
{   
    NEnumDecl* nEnum;
    SEnumDecl* sEnum;
    AccessorContext accessorContext;

    EnumTask(NEnumDecl* nEnum, SEnumDecl* sEnum, AccessorContext accessorContext)
        : nEnum{nEnum}, sEnum{sEnum}, accessorContext{accessorContext}
    {}
public:
    static void Register(NEnumDecl* nEnum, SEnumDecl* sEnum, AccessorContext accessorContext, PhaseManager& phaseManager);

    // Inherited via IBuildTypeSymbolTask
    void BuildTypeSymbol(BuildTypeSymbolContext& context) override;
};

class EnumElemTask
    : public IBuildTypeSymbolTask
{
    NEnumElemDecl* nEnumElem;
    SEnumElemDecl* sEnumElem;

    EnumElemTask(NEnumElemDecl* nEnumElem, SEnumElemDecl* sEnumElem)
        : nEnumElem{nEnumElem}, sEnumElem{sEnumElem}
    {}

public:
    static void Register(NEnumElemDecl* nEnumElem, SEnumElemDecl* sEnumElem, PhaseManager& phaseManager);
    void BuildTypeSymbol(BuildTypeSymbolContext& context) override;
};

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

    // Inherited via IBuildTypeDependentSymbolTask
    void BuildTypeDependentSymbol(BuildTypeDependentSymbolContext& context) override;
};


} // namespace SyntaxIR0Translator
} // namespace Citron

