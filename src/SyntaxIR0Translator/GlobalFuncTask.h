#pragma once
#include <memory>

#include "TranslationTasks.h"

namespace Citron {

class NNamespaceDecl;
class SGlobalFuncDecl;
class NGlobalFuncDecl;
using NFactoryPtr = std::shared_ptr<class NFactory>;

namespace SyntaxIR0Translator {

class PhaseManager;

class GlobalFuncTask 
    : public IBuildTypeDependentSymbolTask
    , public ITranslateBodyTask
{
    NNamespaceDecl* nOuter;    
    SGlobalFuncDecl* syntax;
    NFactoryPtr nFactory;

    NGlobalFuncDecl* nGFuncDecl;

    GlobalFuncTask(NNamespaceDecl* nOuter, SGlobalFuncDecl* syntax, const NFactoryPtr& nFactory)
        : nOuter{nOuter}, syntax{syntax}, nFactory{nFactory}, nGFuncDecl{nullptr}
    {
    }

public:
    static void Register(NNamespaceDecl* nOuter, SGlobalFuncDecl* syntax, const NFactoryPtr& nFactory, PhaseManager& phaseManager);

    void BuildTypeDependentSymbol(BuildTypeDependentSymbolContext& context) override;
    void TranslateBody(TranslateBodyContext& context) override;
};

} // namespace SyntaxIR0Translator

} // namespace Citron