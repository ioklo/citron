#pragma once

#include "TextAnalysisConfig.h"

#include <optional>
#include <vector>

#include "Syntax/Syntax.h"

#include "Lexer.h"

namespace Citron {

class SFactory;

std::optional<std::vector<STypeExp*>> ParseTypeArgs(Lexer* lexer, SFactory& factory);
STypeExp_Id* ParseIdTypeExp(Lexer* lexer, SFactory& factory);
STypeExp_Nullable* ParseNullableTypeExp(Lexer* lexer, SFactory& factory);
STypeExp_BoxPtr* ParseBoxPtrTypeExp(Lexer* lexer, SFactory& factory);
STypeExp* ParseLocalPtrTypeExp(Lexer* lexer, SFactory& factory);
STypeExp* ParseParenTypeExp(Lexer* lexer, SFactory& factory);
STypeExp* ParseIdChainTypeExp(Lexer* lexer, SFactory& factory);

// SFuncTypeExp* ParseFuncTypeExp(Lexer* lexer, SFactory& factory);
// STupleTypeExp* ParseTupleTypeExp(Lexer* lexer, SFactory& factory);

STypeExp_Local* ParseLocalTypeExp(Lexer* lexer, SFactory& factory);
TEXTANALYSIS_API STypeExp* ParseTypeExp(Lexer* lexer, SFactory& factory);
}