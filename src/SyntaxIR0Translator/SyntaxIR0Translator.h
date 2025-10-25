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

class NModule;
class EModule;
class RFactory;

SYNTAXIR0TRANSLATOR_API
std::expected<NModule*, DiagPtr> Translate(
    EName moduleName,
    const std::vector<SScript*>& scripts,
    const std::vector<EModule*>& referenceModules,
    RFactory& factory);

} // namespace Citron