#pragma once
#include <memory>
#include "Infra/Ref.h"
#include "TranslationTasks.h"

namespace Citron {

class RStructDecl;
class RStructFuncDecl;
class SStructFuncDecl;
using RFactoryPtr = std::shared_ptr<class RFactory>;

class PhaseManager;

class StructFuncTask
    : public IBuildNonTypeSymbolTask
    , public ITranslateBodyTask
{
    RStructDecl* rStruct;
    SStructFuncDecl* sStructFunc;
    RFactoryPtr rFactory;

    RStructFuncDecl* rStructFunc;

private:
    StructFuncTask(RStructDecl* rStruct, SStructFuncDecl* sStructFunc, TakeRef<RFactoryPtr> rFactory)
        : rStruct{rStruct}, sStructFunc{sStructFunc}, rFactory{rFactory.Take()}, rStructFunc{nullptr}
    {
    }

public:
    static void Register(RStructDecl* rStructDecl, SStructFuncDecl* sStructDecl, TakeRef<RFactoryPtr> rFactory, PhaseManager& phaseManager);

    std::expected<void, DiagPtr> BuildNonTypeSymbol(BuildNonTypeSymbolContext& context) override;
    std::expected<MFuncBody, DiagPtr> TranslateBody(TranslateBodyContext& context) override;
};

} // namespace Citron