#pragma once
#include "SyntaxIR0TranslatorConfig.h"

#include <optional>
#include <memory>
#include <vector>
#include <expected>

#include "Logging/Diag.h"
#include "Syntax/Syntax.h"
#include "Symbol/MNames.h"

namespace Citron {

class NModule;
class MModule;
class RFactory;

SYNTAXIR0TRANSLATOR_API
std::expected<NModule*, DiagPtr> Translate(
    MName moduleName,
    const std::vector<SScript*>& scripts,
    const std::vector<MModule*>& referenceModules,
    RFactory& factory);

} // namespace Citron