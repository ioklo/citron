#pragma once
#include <memory>
#include <expected>

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
    std::expected<MFuncBody, DiagPtr> TranslateBody(TranslateBodyContext& context) override;
};

} // namespace SyntaxIR0Translator

} // namespace Citron