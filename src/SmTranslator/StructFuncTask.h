#pragma once
#include <memory>
#include "TranslationTasks.h"

namespace Citron {

class NStructDecl;
class NStructFuncDecl;
class SStructFuncDecl;
using RFactoryPtr = std::shared_ptr<class RFactory>;
using NFactoryPtr = std::shared_ptr<class NFactory>;

class PhaseManager;

class StructFuncTask
    : public IBuildTypeDependentSymbolTask
    , public ITranslateBodyTask
{
    NStructDecl* nStruct;
    SStructFuncDecl* sStruct;
    RFactoryPtr rFactory;
    NFactoryPtr nFactory;

    NStructFuncDecl* nStructFunc;

private:
    StructFuncTask(NStructDecl* nStruct, SStructFuncDecl* sStruct, const RFactoryPtr& rFactory, const NFactoryPtr& nFactory)
        : nStruct{nStruct}, sStruct{sStruct}, rFactory{rFactory}, nFactory{nFactory}, nStructFunc{nullptr}
    {
    }

public:
    static void Register(NStructDecl* nStructDecl, SStructFuncDecl* sStructDecl, const RFactoryPtr& rFactory, const NFactoryPtr& nFactory, PhaseManager& phaseManager);

    std::expected<void, DiagPtr> BuildTypeDependentSymbol(BuildTypeDependentSymbolContext& context) override;
    std::expected<MFuncBody, DiagPtr> TranslateBody(TranslateBodyContext& context) override;
};

} // namespace Citron