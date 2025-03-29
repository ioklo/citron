export module Citron.TypeExpParser;

import "TextAnalysisConfig.h";

import <optional>;
import <vector>;

import Citron.Syntax;
import Citron.Lexer;

namespace Citron {

export std::optional<std::vector<STypeExpPtr>> ParseTypeArgs(Lexer* lexer);
export std::shared_ptr<STypeExp_Id> ParseIdTypeExp(Lexer* lexer);
export std::shared_ptr<STypeExp_Nullable> ParseNullableTypeExp(Lexer* lexer);
export std::shared_ptr<STypeExp_BoxPtr> ParseBoxPtrTypeExp(Lexer* lexer);
export STypeExpPtr ParseLocalPtrTypeExp(Lexer* lexer);
export STypeExpPtr ParseParenTypeExp(Lexer* lexer);
export STypeExpPtr ParseIdChainTypeExp(Lexer* lexer);

// std::shared_ptr<SFuncTypeExp> ParseFuncTypeExp(Lexer* lexer);
// std::shared_ptr<STupleTypeExp> ParseTupleTypeExp(Lexer* lexer);

export std::shared_ptr<STypeExp_Local> ParseLocalTypeExp(Lexer* lexer);
export TEXTANALYSIS_API STypeExpPtr ParseTypeExp(Lexer* lexer);
}