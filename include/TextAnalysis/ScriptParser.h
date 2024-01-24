#pragma once
#include "TextAnalysisConfig.h"

#include <optional>

namespace Citron {

class ScriptSyntax;
class Lexer;

TEXT_ANALYSIS_API std::optional<ScriptSyntax> ParseScript(Lexer* lexer);

}