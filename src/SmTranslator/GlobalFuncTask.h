#pragma once
#include <memory>
#include <expected>

#include "Infra/Ref.h"
#include "TranslationTasks.h"

namespace Citron {

class RNamespace;
class SGlobalFuncDecl;
class RGlobalFuncDecl;
using RFactoryPtr = std::shared_ptr<class RFactory>;
using SmDeclContextPtr = std::shared_ptr<class SmDeclContext>;

class SmPhaseManager;

class GlobalFuncTask 
    : public IBuildNonTypeSymbolTask
    , public ITranslateBodyTask
{
    SmDeclContextPtr outerDeclContext;
    RNamespace* rOuter;    
    SGlobalFuncDecl* syntax;
    RFactoryPtr rFactory;

    RGlobalFuncDecl* rFuncDecl;

    GlobalFuncTask(TakeRef<SmDeclContextPtr> outerDeclContext, RNamespace* rOuter, SGlobalFuncDecl* syntax, TakeRef<RFactoryPtr> rFactory)
        : outerDeclContext{outerDeclContext.Take()}, rOuter{rOuter}, syntax{syntax}, rFactory{rFactory.Take()}, rFuncDecl{nullptr}
    {
    }

public:
    static void Register(TakeRef<SmDeclContextPtr> outerDeclContext, RNamespace* rOuter, SGlobalFuncDecl* syntax, TakeRef<RFactoryPtr> rFactory, SmPhaseManager& phaseManager);

    std::expected<void, DiagPtr> BuildNonTypeSymbol(BuildNonTypeSymbolContext& context) override;
    std::expected<MFuncBody, DiagPtr> TranslateBody(TranslateBodyContext& context) override;
};

} // namespace Citron