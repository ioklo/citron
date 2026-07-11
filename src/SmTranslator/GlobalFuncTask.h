#pragma once
#include <memory>
#include <expected>

#include "Infra/Ref.h"
#include "TranslationTasks.h"

namespace Citron {

class RNamespaceDecl;
class SGlobalFuncDecl;
class RGlobalFuncDecl;
using RFactoryPtr = std::shared_ptr<class RFactory>;

class PhaseManager;

class GlobalFuncTask 
    : public IBuildTypeDependentSymbolTask
    , public ITranslateBodyTask
{
    RNamespaceDecl* rOuter;    
    SGlobalFuncDecl* syntax;
    RFactoryPtr rFactory;

    RGlobalFuncDecl* rFuncDecl;

    GlobalFuncTask(RNamespaceDecl* rOuter, SGlobalFuncDecl* syntax, TakeRef<RFactoryPtr> rFactory)
        : rOuter{rOuter}, syntax{syntax}, rFactory{rFactory.Take()}, rFuncDecl{nullptr}
    {
    }

public:
    static void Register(RNamespaceDecl* rOuter, SGlobalFuncDecl* syntax, TakeRef<RFactoryPtr> rFactory, PhaseManager& phaseManager);

    std::expected<void, DiagPtr> BuildTypeDependentSymbol(BuildTypeDependentSymbolContext& context) override;
    std::expected<MFuncBody, DiagPtr> TranslateBody(TranslateBodyContext& context) override;
};

} // namespace Citron