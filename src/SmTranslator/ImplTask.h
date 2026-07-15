#pragma once

#include "TranslationTasks.h"

namespace Citron {

class PhaseManager;
class SImplDecl;
class RDecl;

class ImplTask
    : public IPostBuildNonTypeSymbolTask
    , public ITranslateBodyTask
{
    SImplDecl* implDecl;
    RDecl* outer;

public:
    static void Register(SImplDecl* implDecl, RDecl* outer, PhaseManager& phaseManager);
    ImplTask(SImplDecl* implDecl, RDecl* outer) : implDecl{implDecl}, outer{outer} {}

public: // from IPostBuildNonTypeSymbolTask
    std::expected<void, DiagPtr> PostBuildNonTypeSymbol(PostBuildNonTypeSymbolContext& context) final;

public: // from ITranslateBodyTask
    std::expected<MFuncBody, DiagPtr> TranslateBody(TranslateBodyContext& context) final;

};

} // namespace Citron
