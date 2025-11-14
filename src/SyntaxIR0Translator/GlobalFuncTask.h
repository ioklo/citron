#pragma once
#include "TranslationTasks.h"

namespace Citron {

class NNamespaceDecl;
class SGlobalFuncDecl;
class NGlobalFuncDecl;

namespace SyntaxIR0Translator {

class PhaseManager;

class GlobalFuncTask 
    : public IBuildTypeDependentSymbolTask
    , public ITranslateBodyTask
{
    NGlobalFuncDecl* nGFuncDecl;
    SGlobalFuncDecl* syntax;

    GlobalFuncTask(NGlobalFuncDecl* nGFuncDecl, SGlobalFuncDecl* syntax)
        : nGFuncDecl{nGFuncDecl}, syntax{syntax}
    {
    }

public:
    static void Register(NGlobalFuncDecl* nGFuncDecl, SGlobalFuncDecl* syntax, PhaseManager& phaseManager);

    void BuildTypeDependentSymbol(BuildTypeDependentSymbolContext& context) override;
    void TranslateBody(TranslateBodyContext& context) override;
};

} // namespace SyntaxIR0Translator

} // namespace Citron