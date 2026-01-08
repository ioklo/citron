#pragma once

#include "TextAnalysisConfig.h"

#include <optional>
#include <vector>

#include "Syntax/Syntax.h"

#include "Lexer.h"

namespace Citron {

class SFactory;

std::optional<std::vector<STypeExp*>> ParseTypeArgs(Lexer* lexer, SFactory& factory);

// 지역변수 선언, forstmt 선언에서의 typeExp 파싱
SVarDeclType* ParseVarDeclTypeExp(Lexer* lexer, SFactory& factory);

struct FuncParamType
{
    bool bRef;
    STypeExp* typeExp;
};

std::optional<FuncParamType> ParseFuncParamTypeExp(Lexer* lexer, SFactory& factory);

// 일반 typeExp 파싱
TEXTANALYSIS_API STypeExp* ParseTypeExp(Lexer* lexer, SFactory& factory);
}