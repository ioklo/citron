#pragma once
#include "TextAnalysisConfig.h"

#include <optional>

namespace Citron {

class ScriptSyntax;
class Lexer;

TEXTANALYSIS_API std::optional<ScriptSyntax> ParseScript(Lexer* lexer);

}