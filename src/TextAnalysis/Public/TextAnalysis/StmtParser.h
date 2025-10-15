#pragma once

#include "TextAnalysisConfig.h"

#include <optional>
#include <vector>

#include "Syntax/Syntax.h"

#include "Lexer.h"

namespace Citron {

TEXTANALYSIS_API SStmt* ParseStmt(Lexer* lexer, SFactory& factory);
std::optional<std::vector<SStmt*>> ParseBody(Lexer* lexer, SFactory& factory);

}
