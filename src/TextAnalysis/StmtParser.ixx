export module Citron.StmtParser;

import "TextAnalysisConfig.h";

import <optional>;
import <vector>;

import Citron.Syntax;
import Citron.Lexer;

namespace Citron {

export TEXTANALYSIS_API SStmtPtr ParseStmt(Lexer* lexer);
export std::optional<std::vector<SStmtPtr>> ParseBody(Lexer* lexer);

}
