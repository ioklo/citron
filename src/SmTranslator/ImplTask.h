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
    SImplDecl* sImplDecl;
    RDecl* rOuter;

public:
    static void Register(SImplDecl* sImplDecl, RDecl* rOuter, PhaseManager& phaseManager);
    ImplTask(SImplDecl* sImplDecl, RDecl* rOuter) : sImplDecl{sImplDecl}, rOuter{rOuter} {}

public: // from IPostBuildNonTypeSymbolTask
    std::expected<void, DiagPtr> PostBuildNonTypeSymbol(PostBuildNonTypeSymbolContext& context) final;

public: // from ITranslateBodyTask
    std::expected<MFuncBody, DiagPtr> TranslateBody(TranslateBodyContext& context) final;

};

} // namespace Citron
