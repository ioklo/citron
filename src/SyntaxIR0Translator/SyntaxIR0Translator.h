#pragma once
#include <optional>
#include <memory>
#include <vector>

#include "SyntaxIR0TranslatorConfig.h"
#include <IR0/RProgram.h>
#include <IR0/RStmt.h>
#include <Symbol/MNames.h>
#include <Syntax/Syntax.h>

namespace Citron {

SYNTAXIR0TRANSLATOR_API
std::optional<RProgram> Translate(
    MName moduleName,
    std::vector<SScript> scripts,
    std::vector<std::shared_ptr<MModuleDecl>> referenceModules);

}