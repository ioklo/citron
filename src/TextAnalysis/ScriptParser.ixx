export module Citron.ScriptParser;

import "TextAnalysisConfig.h";
import <optional>;

import Citron.Syntax;
import Citron.Lexer;

namespace Citron {

export TEXTANALYSIS_API std::optional<SScript> ParseScript(Lexer* lexer);

}