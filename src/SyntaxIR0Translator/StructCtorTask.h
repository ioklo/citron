#pragma once
#include "TranslationTasks.h"

namespace Citron {

class NStructCtorDecl;
class SStructCtorDecl;

namespace SyntaxIR0Translator {

class PhaseManager;

class StructCtorTask
    : public IBuildTypeDependentSymbolTask
    , public ITranslateBodyTask
{
    NStructCtorDecl* symbol;
    SStructCtorDecl* syntax;

private:
    StructCtorTask(NStructCtorDecl* symbol, SStructCtorDecl* syntax)
        : symbol{symbol}, syntax{syntax}
    {}

public:
    static void Register(NStructCtorDecl* nFuncDecl, SStructCtorDecl* syntax, PhaseManager& phaseManager);

    // Inherited via IBuildTypeDependentSymbolTask
    void BuildTypeDependentSymbol(BuildTypeDependentSymbolContext& context) override;


    // Inherited via ITranslateBodyTask
    void TranslateBody(TranslateBodyContext& context) override;

};

} // namespace SyntaxIR0Translator
} // namespace Citron