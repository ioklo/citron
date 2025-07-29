#pragma once
#include "TextAnalysisConfig.h"

#include <optional>

#include "Syntax/Syntax.h"
#include "Lexer.h"

namespace Citron {

TEXTANALYSIS_API std::optional<SScript> ParseScript(Lexer* lexer);

}