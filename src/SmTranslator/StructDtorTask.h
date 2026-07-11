#pragma once
#include <memory>
#include "Infra/Ref.h"
#include "TranslationTasks.h"

namespace Citron {

class RStructDecl;
class RStructDtorDecl;
class SStructDtorDecl;
class PhaseManager;
using RFactoryPtr = std::shared_ptr<class RFactory>;

class StructDtorTask
    : public IBuildTypeDependentSymbolTask
    , public ITranslateBodyTask
{
    RStructDecl* rStruct;
    SStructDtorDecl* sStructDtor;
    RFactoryPtr rFactory;

    RStructDtorDecl* rStructDtor;

public:
    StructDtorTask(RStructDecl* rStruct, SStructDtorDecl* sStructDtor, TakeRef<RFactoryPtr> rFactory);
    std::expected<void, DiagPtr> BuildTypeDependentSymbol(BuildTypeDependentSymbolContext& context) override;
    std::expected<MFuncBody, DiagPtr> TranslateBody(TranslateBodyContext& context) override;

public:
    static void Register(RStructDecl* rStruct, SStructDtorDecl* sStructDtor, TakeRef<RFactoryPtr> rFactory, PhaseManager& phaseManager);
};

} // namespace Citron