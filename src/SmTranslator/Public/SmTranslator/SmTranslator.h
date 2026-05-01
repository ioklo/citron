#pragma once
#include "SmTranslatorConfig.h"

#include <optional>
#include <memory>
#include <vector>
#include <expected>

#include "Logging/Diag.h"
#include "Syntax/Syntax.h"
#include "ESymbol/ENames.h"

namespace Citron {

class SScript;
class NModule;
class MData;
class EModule;
using LoggerPtr = std::shared_ptr<class Logger>;
using RFactoryPtr = std::shared_ptr<class RFactory>;
using NFactoryPtr = std::shared_ptr<class NFactory>;
using MFactoryPtr = std::shared_ptr<class MFactory>;

struct NModuleMData
{
    NModule* nModule;
    MData* mData;
};

SMTRANSLATOR_API
std::expected<NModuleMData, DiagPtr> TranslateSyntaxToNModuleMData(
    std::string moduleName,
    const std::vector<SScript*>& scripts, // translation units
    const std::vector<EModule*>& referenceModules,
    const LoggerPtr& logger,
    const RFactoryPtr& rFactory,
    const NFactoryPtr& nFactory,
    const MFactoryPtr& mFactory);

} // namespace Citron
