#pragma once
#include "TextAnalysisConfig.h"

#include <optional>

namespace Citron {

class SScript;
class Lexer;

TEXTANALYSIS_API std::optional<SScript> ParseScript(Lexer* lexer);

}