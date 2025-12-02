#pragma once
#include <memory>
#include "TranslationTasks.h"

namespace Citron {

class NStructDecl;
class NStructFuncDecl;
class SStructFuncDecl;
using NFactoryPtr = std::shared_ptr<class NFactory>;

class PhaseManager;

class StructFuncTask
    : public IBuildTypeDependentSymbolTask
    , public ITranslateBodyTask
{
    NStructDecl* nStruct;
    SStructFuncDecl* sStruct;
    NFactoryPtr nFactory;

    NStructFuncDecl* nStructFunc;

private:
    StructFuncTask(NStructDecl* nStruct, SStructFuncDecl* sStruct, const NFactoryPtr& nFactory)
        : nStruct{nStruct}, sStruct{sStruct}, nFactory{nFactory}, nStructFunc{nullptr}
    {
    }

public:
    static void Register(NStructDecl* nStructDecl, SStructFuncDecl* sStructDecl, const NFactoryPtr& nFactory, PhaseManager& phaseManager);

    void BuildTypeDependentSymbol(BuildTypeDependentSymbolContext& context) override;
    std::expected<MFuncBody, DiagPtr> TranslateBody(TranslateBodyContext& context) override;
};

} // namespace Citron