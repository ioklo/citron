#pragma once

#include "TextAnalysisConfig.h"

#include <optional>
#include <vector>

#include "Syntax/Syntax.h"

#include "Lexer.h"

namespace Citron {

class SFactory;

std::optional<std::vector<STypeExp*>> ParseTypeArgs(Lexer* lexer, SFactory& factory);
SVarDeclType* ParseVarDeclTypeExp(Lexer* lexer, SFactory& factory);
TEXTANALYSIS_API STypeExp* ParseTypeExp(Lexer* lexer, SFactory& factory);
}