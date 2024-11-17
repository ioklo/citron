#pragma once
#include <optional>
#include <memory>
#include <vector>

#include "SyntaxIR0TranslatorConfig.h"
#include <IR0/NModule.h>
#include <IR0/NStmt.h>
#include <Symbol/MNames.h>
#include <Symbol/MModule.h>
#include <Syntax/Syntax.h>

namespace Citron {

namespace SyntaxIR0Translator {

SYNTAXIR0TRANSLATOR_API
std::shared_ptr<NModule> Translate(
    MName moduleName,
    std::vector<SScript> scripts,
    std::vector<std::shared_ptr<MModule>> referenceModules,
    RTypeFactory& factory);

} // namespace SyntaxIR0Translator

} // namespace Citron