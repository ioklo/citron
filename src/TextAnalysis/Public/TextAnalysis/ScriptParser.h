#pragma once
#include "TextAnalysisConfig.h"

#include <optional>

#include "Syntax/Syntax.h"
#include "Lexer.h"

namespace Citron {

TEXTANALYSIS_API SScript* ParseScript(Lexer* lexer, SFactory& factory);

}