#pragma once
#include <vector>

#include "TranslationTasks.h"

namespace Citron {

class NStructDecl;
class NStructVarDecl;
class SStructVarDecl;

namespace SyntaxIR0Translator {

class PhaseManager;

class StructVarTask
    : public IBuildTypeDependentSymbolTask
{
    NStructDecl* nOuter;
    std::vector<NStructVarDecl*> symbols;
    SStructVarDecl* syntax;

private:
    StructVarTask(NStructDecl* nOuter, std::vector<NStructVarDecl*>&& symbols, SStructVarDecl* syntax)
        : nOuter{nOuter}, symbols{std::move(symbols)}, syntax{syntax}
    {}

public:
    static void Register(NStructDecl* nOuter, std::vector<NStructVarDecl*>&& symbols, SStructVarDecl* syntax, PhaseManager& phaseManager);
    void BuildTypeDependentSymbol(BuildTypeDependentSymbolContext& context) override;
};

} // namespace SyntaxIR0Translator
} // namespace Citron