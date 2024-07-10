#pragma once
#include "TextAnalysisConfig.h"

#include <optional>
#include <vector>
#include <Syntax/Syntax.h>

namespace Citron {

class Lexer;

TEXTANALYSIS_API std::optional<StmtSyntax> ParseStmt(Lexer* lexer);
std::optional<std::vector<StmtSyntax>> ParseBody(Lexer* lexer);

}
