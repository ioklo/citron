#pragma once
#include "SyntaxIR0TranslatorConfig.h"

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
using RFactoryPtr = std::shared_ptr<class RFactory>;
using NFactoryPtr = std::shared_ptr<class NFactory>;

struct NModuleMData
{
    NModule* nModule;
    MData* mData;
};

SYNTAXIR0TRANSLATOR_API
std::expected<NModuleMData, DiagPtr> TranslateSyntaxToNModuleMData(
    std::string moduleName,
    const std::vector<SScript*>& scripts, // translation units
    const std::vector<EModule*>& referenceModules,
    const RFactoryPtr& rFactory,
    const NFactoryPtr& nFactory);

} // namespace Citron
