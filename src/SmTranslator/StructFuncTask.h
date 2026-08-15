#pragma once
#include <memory>
#include "Infra/Ref.h"
#include "TranslationTasks.h"

namespace Citron {

class RStructDecl;
class RStructFuncDecl;
class SStructFuncDecl;
using RFactoryPtr = std::shared_ptr<class RFactory>;
using SmDeclContextPtr = std::shared_ptr<class SmDeclContext>;

class SmPhaseManager;

class StructFuncTask
    : public IBuildNonTypeSymbolTask
    , public ITranslateBodyTask
{
    SmDeclContextPtr structDeclContext;
    RStructDecl* rStruct;
    SStructFuncDecl* sStructFunc;
    RFactoryPtr rFactory;

    RStructFuncDecl* rStructFunc;

private:
    StructFuncTask(TakeRef<SmDeclContextPtr> structDeclContext, RStructDecl* rStruct, SStructFuncDecl* sStructFunc, TakeRef<RFactoryPtr> rFactory)
        : structDeclContext{structDeclContext.Take()}, rStruct{rStruct}, sStructFunc{sStructFunc}, rFactory{rFactory.Take()}, rStructFunc{nullptr}
    {
    }

public:
    static void Register(TakeRef<SmDeclContextPtr> structDeclContext, RStructDecl* rStructDecl, SStructFuncDecl* sStructDecl, TakeRef<RFactoryPtr> rFactory, SmPhaseManager& phaseManager);

    std::expected<void, DiagPtr> BuildNonTypeSymbol(BuildNonTypeSymbolContext& context) override;
    std::expected<MFuncBody, DiagPtr> TranslateBody(TranslateBodyContext& context) override;
};

} // namespace Citron