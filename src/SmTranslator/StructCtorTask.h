#pragma once
#include <memory>
#include "Infra/Ref.h"
#include "TranslationTasks.h"

namespace Citron {

class RStructDecl;
class RStructCtorDecl;
class SStructCtorDecl;
class PhaseManager;

using RFactoryPtr = std::shared_ptr<class RFactory>;

class StructCtorTask
    : public IBuildNonTypeSymbolTask
    , public ITranslateBodyTask
{
    RStructDecl* rStruct;
    SStructCtorDecl* sStructCtor;

    RStructCtorDecl* rStructCtor;
    RFactoryPtr rFactory;

private:
    StructCtorTask(RStructDecl* rStruct, SStructCtorDecl* sStructCtor, TakeRef<RFactoryPtr> rFactory)
        : rStruct{rStruct}, sStructCtor{sStructCtor}, rStructCtor{nullptr}, rFactory{rFactory.Take()}
    {}

public:
    static void Register(RStructDecl* rStruct, SStructCtorDecl* sStructCtor, TakeRef<RFactoryPtr> rFactory, PhaseManager& phaseManager);
    std::expected<void, DiagPtr> BuildNonTypeSymbol(BuildNonTypeSymbolContext& context) override;
    std::expected<MFuncBody, DiagPtr> TranslateBody(TranslateBodyContext& context) override;

};

} // namespace Citron