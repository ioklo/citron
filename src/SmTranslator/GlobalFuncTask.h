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

class PhaseManager;

class GlobalFuncTask 
    : public IBuildNonTypeSymbolTask
    , public ITranslateBodyTask
{
    RNamespace* rOuter;    
    SGlobalFuncDecl* syntax;
    RFactoryPtr rFactory;

    RGlobalFuncDecl* rFuncDecl;

    GlobalFuncTask(RNamespace* rOuter, SGlobalFuncDecl* syntax, TakeRef<RFactoryPtr> rFactory)
        : rOuter{rOuter}, syntax{syntax}, rFactory{rFactory.Take()}, rFuncDecl{nullptr}
    {
    }

public:
    static void Register(RNamespace* rOuter, SGlobalFuncDecl* syntax, TakeRef<RFactoryPtr> rFactory, PhaseManager& phaseManager);

    std::expected<void, DiagPtr> BuildNonTypeSymbol(BuildNonTypeSymbolContext& context) override;
    std::expected<MFuncBody, DiagPtr> TranslateBody(TranslateBodyContext& context) override;
};

} // namespace Citron