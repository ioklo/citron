#pragma once
#include "TranslationTasks.h"

namespace Citron {

class NStructFuncDecl;
class SStructFuncDecl;

namespace SyntaxIR0Translator {

class PhaseManager;

class StructFuncTask
    : public IBuildTypeDependentSymbolTask
    , public ITranslateBodyTask
{
    NStructFuncDecl* nFuncDecl;
    SStructFuncDecl* syntax;

private:
    StructFuncTask(NStructFuncDecl* nFuncDecl, SStructFuncDecl* syntax);

public:
    static void Register(NStructFuncDecl* nFuncDecl, SStructFuncDecl* syntax, PhaseManager& phaseManager);

    void BuildTypeDependentSymbol(BuildTypeDependentSymbolContext& context) override;
    void TranslateBody(TranslateBodyContext& context) override;
};

} // namespace SyntaxIR0Translator
} // namespace Citron