#pragma once
#include <optional>
#include <memory>
#include <vector>

#include "SyntaxIR0TranslatorConfig.h"
#include <IR0/RModuleDecl.h>
#include <IR0/RStmt.h>
#include <Symbol/MNames.h>
#include <Symbol/MModuleDecl.h>
#include <Syntax/Syntax.h>

namespace Citron {

namespace SyntaxIR0Translator {

SYNTAXIR0TRANSLATOR_API
std::shared_ptr<RModuleDecl> Translate(
    MName moduleName,
    std::vector<SScript> scripts,
    std::vector<std::shared_ptr<MModuleDecl>> referenceModules);

} // namespace SyntaxIR0Translator

} // namespace Citron