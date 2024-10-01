#pragma once
#include "TextAnalysisConfig.h"

#include <optional>
#include <vector>
#include <Syntax/Syntax.h>

namespace Citron {

class Lexer;

std::optional<std::vector<STypeExpPtr>> ParseTypeArgs(Lexer* lexer);
std::shared_ptr<SIdTypeExp> ParseIdTypeExp(Lexer* lexer);
std::shared_ptr<SNullableTypeExp> ParseNullableTypeExp(Lexer * lexer);
std::shared_ptr<SBoxPtrTypeExp> ParseBoxPtrTypeExp(Lexer* lexer);
STypeExpPtr ParseLocalPtrTypeExp(Lexer* lexer);
STypeExpPtr ParseParenTypeExp(Lexer* lexer);
STypeExpPtr ParseIdChainTypeExp(Lexer* lexer);

// std::shared_ptr<SFuncTypeExp> ParseFuncTypeExp(Lexer* lexer);
// std::shared_ptr<STupleTypeExp> ParseTupleTypeExp(Lexer* lexer);

std::shared_ptr<SLocalTypeExp> ParseLocalTypeExp(Lexer* lexer);
TEXTANALYSIS_API STypeExpPtr ParseTypeExp(Lexer* lexer);
}