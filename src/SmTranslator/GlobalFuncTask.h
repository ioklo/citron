#pragma once
#include <memory>
#include <expected>

#include "TranslationTasks.h"

namespace Citron {

class NNamespaceDecl;
class SGlobalFuncDecl;
class NGlobalFuncDecl;
using RFactoryPtr = std::shared_ptr<class RFactory>;
using NFactoryPtr = std::shared_ptr<class NFactory>;

class PhaseManager;

class GlobalFuncTask 
    : public IBuildTypeDependentSymbolTask
    , public ITranslateBodyTask
{
    NNamespaceDecl* nOuter;    
    SGlobalFuncDecl* syntax;
    RFactoryPtr rFactory;
    NFactoryPtr nFactory;

    NGlobalFuncDecl* nGFuncDecl;

    GlobalFuncTask(NNamespaceDecl* nOuter, SGlobalFuncDecl* syntax, const RFactoryPtr& rFactory, const NFactoryPtr& nFactory)
        : nOuter{nOuter}, syntax{syntax}, rFactory{rFactory}, nFactory {
        nFactory
    }, nGFuncDecl{nullptr}
    {
    }

public:
    static void Register(NNamespaceDecl* nOuter, SGlobalFuncDecl* syntax, const RFactoryPtr& rFactory, const NFactoryPtr& nFactory, PhaseManager& phaseManager);

    std::expected<void, DiagPtr> BuildTypeDependentSymbol(BuildTypeDependentSymbolContext& context) override;
    std::expected<MFuncBody, DiagPtr> TranslateBody(TranslateBodyContext& context) override;
};

} // namespace Citron