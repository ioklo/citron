#pragma once
#include <memory>
#include <expected>
#include "Infra/Ref.h"
#include "RSymbol/RImplTraitDeclOuter.h"
#include "TranslationTasks.h"

namespace Citron {

class SImplTraitDecl;
class SImplTraitFuncDecl;
class RDecl;
class RImplTraitDecl;
class RImplTraitDeclOuter;
using RFactoryPtr = std::shared_ptr<class RFactory>; 

class PhaseManager;
using SmDeclContextPtr = std::shared_ptr<class SmDeclContext>;

class ImplTraitTask
    : public IPostBuildNonTypeSymbolTask
{
    SImplTraitDecl* sImplDecl;
    SmDeclContextPtr outerDeclContext;
    RImplTraitDeclOuter rOuter;

    RImplTraitDecl* rImplTraitDecl;

    RFactoryPtr rFactory;

public:
    static void Register(SImplTraitDecl* sImplDecl, TakeRef<SmDeclContextPtr> outerDeclContext, RImplTraitDeclOuter rOuter, TakeRef<RFactoryPtr> rFactory, PhaseManager& phaseManager);
    ImplTraitTask(SImplTraitDecl    * sImplDecl, TakeRef<SmDeclContextPtr> outerDeclContext, RImplTraitDeclOuter rOuter, TakeRef<RFactoryPtr> rFactory)
        : sImplDecl{sImplDecl}, outerDeclContext{outerDeclContext.Take()}, rOuter{rOuter}, rImplTraitDecl{nullptr}, rFactory{rFactory.Take()} {}

    std::expected<void, DiagPtr> HandleImplTraitFuncDecl(SImplTraitFuncDecl* sImplTraitFuncDecl, PostBuildNonTypeSymbolContext& context);

public: // from IPostBuildNonTypeSymbolTask
    std::expected<void, DiagPtr> PostBuildNonTypeSymbol(PostBuildNonTypeSymbolContext& context) final;

};

} // namespace Citron
