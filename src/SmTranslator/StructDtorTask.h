#pragma once
#include <memory>
#include "Infra/Ref.h"
#include "TranslationTasks.h"

namespace Citron {

class RStructDecl;
class RStructDtorDecl;
class SStructDtorDecl;
class SmPhaseManager;
using RFactoryPtr = std::shared_ptr<class RFactory>;
using SmDeclContextPtr = std::shared_ptr<class SmDeclContext>;

class StructDtorTask
    : public IBuildNonTypeSymbolTask
    , public ITranslateBodyTask
{
    SmDeclContextPtr structDeclContext;
    RStructDecl* rStruct;
    SStructDtorDecl* sStructDtor;
    RFactoryPtr rFactory;

    RStructDtorDecl* rStructDtor;

public:
    StructDtorTask(TakeRef<SmDeclContextPtr> structDeclContext, RStructDecl* rStruct, SStructDtorDecl* sStructDtor, TakeRef<RFactoryPtr> rFactory);
    std::expected<void, DiagPtr> BuildNonTypeSymbol(BuildNonTypeSymbolContext& context) override;
    std::expected<MFuncBody, DiagPtr> TranslateBody(TranslateBodyContext& context) override;

public:
    static void Register(TakeRef<SmDeclContextPtr> structDeclContext, RStructDecl* rStruct, SStructDtorDecl* sStructDtor, TakeRef<RFactoryPtr> rFactory, SmPhaseManager& phaseManager);
};

} // namespace Citron