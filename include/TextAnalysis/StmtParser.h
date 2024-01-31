#pragma once
#include "TextAnalysisConfig.h"

#include <optional>
#include <vector>
#include <Syntax/StmtSyntaxes.h>

namespace Citron {

class Lexer;

TEXT_ANALYSIS_API std::optional<StmtSyntax> ParseStmt(Lexer* lexer);
std::optional<std::vector<StmtSyntax>> ParseBody(Lexer* lexer);

}
