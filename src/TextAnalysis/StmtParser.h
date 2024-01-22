#pragma once

#include <optional>
#include <vector>
#include "Syntax/StmtSyntaxes.h"

namespace Citron {

class Lexer;

std::optional<StmtSyntax> ParseStmt(Lexer* lexer);
std::optional<std::vector<StmtSyntax>> ParseBody(Lexer* lexer);

}
