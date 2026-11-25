#pragma once
#include "SmTranslatorConfig.h"

#include <memory>
#include <expected>
#include <span>

#include "Infra/RefPtr.h"
#include "Syntax/Syntax.h"

namespace Citron {

using DiagPtr = std::shared_ptr<struct Diag>;
class SScript;
class RModule;
class MData;
class EModule;
class Logger;
class RFactory;
class MFactory;

struct SmTranslationResult
{
    RModule* nModule;
    MData* mData;
};

SMTRANSLATOR_API
std::expected<SmTranslationResult, DiagPtr> TranslateSyntax(
    std::string moduleName,
    std::span<SScript*> scripts, // translation units
    std::span<EModule*> referenceModules,
    InRefPtr<Logger> logger,
    InRefPtr<RFactory> rFactory,
    InRefPtr<MFactory> mFactory);

} // namespace Citron
