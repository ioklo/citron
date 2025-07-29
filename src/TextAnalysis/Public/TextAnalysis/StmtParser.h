#pragma once

#include "TextAnalysisConfig.h"

#include <optional>
#include <vector>

#include "Syntax/Syntax.h"

#include "Lexer.h"

namespace Citron {

TEXTANALYSIS_API SStmtPtr ParseStmt(Lexer* lexer);
std::optional<std::vector<SStmtPtr>> ParseBody(Lexer* lexer);

}
