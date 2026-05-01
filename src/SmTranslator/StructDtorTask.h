#pragma once
#include <memory>
#include "TranslationTasks.h"

namespace Citron {

class NStructDecl;
class NStructDtorDecl;
class SStructDtorDecl;

using NFactoryPtr = std::shared_ptr<class NFactory>;
class PhaseManager;

class StructDtorTask
    : public IBuildTypeDependentSymbolTask
    , public ITranslateBodyTask
{
    NStructDecl* nStruct;
    SStructDtorDecl* sStructDtor;
    NFactoryPtr nFactory;

    NStructDtorDecl* nStructDtor;

public:
    StructDtorTask(NStructDecl* nStruct, SStructDtorDecl* sStructDtor, const NFactoryPtr& nFactory);
    std::expected<void, DiagPtr> BuildTypeDependentSymbol(BuildTypeDependentSymbolContext& context) override;
    std::expected<MFuncBody, DiagPtr> TranslateBody(TranslateBodyContext& context) override;

public:
    static void Register(NStructDecl* nStruct, SStructDtorDecl* sStructDtor, const NFactoryPtr& nFactory, PhaseManager& phaseManager);
};

} // namespace Citron