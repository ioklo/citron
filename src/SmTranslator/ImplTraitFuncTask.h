#pragma once
#include <expected>
#include <memory>
#include "Infra/Ref.h"
#include "TranslationTasks.h"

namespace Citron {

class SImplTraitFuncDecl;

class RImplTraitFuncDecl;
using RFactoryPtr = std::shared_ptr<class RFactory>;

class SmPhaseManager;
class TranslateBodyContext;
using SmDeclContextPtr = std::shared_ptr<class SmDeclContext>;

class ImplTraitFuncTask
    : public ITranslateBodyTask
{
    SmDeclContextPtr outerDeclContext;
    RImplTraitFuncDecl* rImplTraitFuncDecl;
    SImplTraitFuncDecl* sImplTraitFuncDecl;
    RFactoryPtr rFactory;

public:
    static void Register(TakeRef<SmDeclContextPtr> outerDeclContext, RImplTraitFuncDecl* rImplTraitFuncDecl, SImplTraitFuncDecl* sImplTraitFuncDecl, TakeRef<RFactoryPtr> rFactory, SmPhaseManager& phaseManager);
    ImplTraitFuncTask(TakeRef<SmDeclContextPtr> outerDeclContext, RImplTraitFuncDecl* rImplTraitFuncDecl, SImplTraitFuncDecl* sImplTraitFuncDecl, TakeRef<RFactoryPtr> rFactory);

    std::expected<MFuncBody, DiagPtr> TranslateBody(TranslateBodyContext& context) final;
};


} // namespace Citron
