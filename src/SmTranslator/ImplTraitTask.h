#pragma once
#include <memory>
#include <expected>
#include "Infra/Ref.h"
#include "RSymbol/RImplTraitDeclOuter.h"
#include "TranslationTasks.h"

namespace Citron {

using DiagPtr = std::shared_ptr<struct Diag>;
class SImplTraitDecl;
class SImplTraitFuncDecl;
class RDecl;
class RImplTraitDecl;
class RImplTraitDeclOuter;
using RFactoryPtr = std::shared_ptr<class RFactory>; 

class SmPhaseManager;
using SmDeclContextPtr = std::shared_ptr<class SmDeclContext>;

class ImplTraitTask
    : public IPostBuildNonTypeSymbolTask
{
    SImplTraitDecl* sImplDecl;
    SmDeclContextPtr outerDeclContext;
    RImplTraitDeclOuter rOuter;
    RFactoryPtr rFactory;
    
private: // 추후에 만들어짐
    RImplTraitDecl* rImplTraitDecl;

public:
    static void Register(SImplTraitDecl* sImplDecl, TakeRef<SmDeclContextPtr> outerDeclContext, RImplTraitDeclOuter rOuter, TakeRef<RFactoryPtr> rFactory, SmPhaseManager& phaseManager);
    ImplTraitTask(SImplTraitDecl* sImplDecl, TakeRef<SmDeclContextPtr> outerDeclContext, RImplTraitDeclOuter rOuter, TakeRef<RFactoryPtr> rFactory)
        : sImplDecl{sImplDecl}, outerDeclContext{outerDeclContext.Take()}, rOuter{rOuter}, rImplTraitDecl{nullptr}, rFactory{rFactory.Take()} {}

    std::expected<void, DiagPtr> HandleImplTraitFuncDecl(SImplTraitFuncDecl* sImplTraitFuncDecl, PostBuildNonTypeSymbolContexts& contexts);
    std::expected<void, DiagPtr> CheckTarget();
    std::expected<void, DiagPtr> CheckTraitConformance();

public: // from IPostBuildNonTypeSymbolTask
    std::expected<void, DiagPtr> PostBuildNonTypeSymbol(PostBuildNonTypeSymbolContexts& context) final;

};

} // namespace Citron
