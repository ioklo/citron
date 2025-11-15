#pragma once
#include <memory>

#include "TranslationTasks.h"

namespace Citron {

class NStructDecl;
class NStructVarDecl;
class SStructVarDecl;
using NFactoryPtr = std::shared_ptr<class NFactory>;

namespace SyntaxIR0Translator {

class PhaseManager;

class StructVarTask
    : public IBuildTypeDependentSymbolTask
{
    NStructDecl* nStruct;
    SStructVarDecl* sStructVar;
    NFactoryPtr nFactory;

private:
    StructVarTask(NStructDecl* nStruct, SStructVarDecl* sStructVar, const NFactoryPtr& nFactory)
        : nStruct{nStruct}, sStructVar{sStructVar}, nFactory{nFactory}
    {}

public:
    static void Register(NStructDecl* nOuter, SStructVarDecl* syntax, const NFactoryPtr& nFactory, PhaseManager& phaseManager);
    void BuildTypeDependentSymbol(BuildTypeDependentSymbolContext& context) override;
};

} // namespace SyntaxIR0Translator
} // namespace Citron