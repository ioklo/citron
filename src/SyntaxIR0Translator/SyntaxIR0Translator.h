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
class RTypeFactory;

SYNTAXIR0TRANSLATOR_API
std::expected<std::shared_ptr<NModule>, DiagPtr> Translate(
    MName moduleName,
    std::vector<SScript> scripts,
    std::vector<std::shared_ptr<MModule>> referenceModules,
    RTypeFactory& factory);

} // namespace Citron