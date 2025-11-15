#pragma once
#include <memory>

#include "TranslationTasks.h"

namespace Citron {

class NStructDecl;
class NStructCtorDecl;
class SStructCtorDecl;
using NFactoryPtr = std::shared_ptr<class NFactory>;

namespace SyntaxIR0Translator {

class PhaseManager;

class StructCtorTask
    : public IBuildTypeDependentSymbolTask
    , public ITranslateBodyTask
{
    NStructDecl* nStruct;
    SStructCtorDecl* sStructCtor;
    NFactoryPtr nFactory;

    NStructCtorDecl* nStructCtor;

private:
    StructCtorTask(NStructDecl* nStruct, SStructCtorDecl* sStructCtor, const NFactoryPtr& nFactory)
        : nStruct{nStruct}, sStructCtor{sStructCtor}, nFactory{nFactory}
    {}

public:
    static void Register(NStructDecl* nStruct, SStructCtorDecl* sStructCtor, const NFactoryPtr& nFactory, PhaseManager& phaseManager);
    void BuildTypeDependentSymbol(BuildTypeDependentSymbolContext& context) override;
    std::expected<MFuncBody, DiagPtr> TranslateBody(TranslateBodyContext& context) override;

};

} // namespace SyntaxIR0Translator
} // namespace Citron