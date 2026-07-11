#pragma once
#include "SmTranslatorConfig.h"

#include <optional>
#include <memory>
#include <vector>
#include <expected>

#include "Infra/Ref.h"
#include "Logging/Diag.h"
#include "Syntax/Syntax.h"

namespace Citron {

class SScript;
class RModule;
class MData;
class EModule;
using LoggerPtr = std::shared_ptr<class Logger>;
using RFactoryPtr = std::shared_ptr<class RFactory>;
using MFactoryPtr = std::shared_ptr<class MFactory>;

struct SmTranslationResult
{
    RModule* nModule;
    MData* mData;
};

SMTRANSLATOR_API
std::expected<SmTranslationResult, DiagPtr> TranslateSyntax(
    std::string moduleName,
    const std::vector<SScript*>& scripts, // translation units
    const std::vector<EModule*>& referenceModules,
    TakeRef<LoggerPtr> logger,
    TakeRef<RFactoryPtr> rFactory,
    TakeRef<MFactoryPtr> mFactory);

} // namespace Citron
