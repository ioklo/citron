#pragma once

#include <optional>
#include <vector>
#include <Syntax/TypeExpSyntaxes.h>

namespace Citron {

class Lexer;

std::optional<std::vector<TypeExpSyntax>> ParseTypeArgs(Lexer* lexer);
std::optional<IdTypeExpSyntax> ParseIdTypeExp(Lexer* lexer);
std::optional<NullableTypeExpSyntax> ParseNullableTypeExp(Lexer * lexer);
std::optional<BoxPtrTypeExpSyntax> ParseBoxPtrTypeExp(Lexer* lexer);
std::optional<LocalPtrTypeExpSyntax> ParseLocalPtrTypeExp(Lexer* lexer);
std::optional<TypeExpSyntax> ParseParenTypeExp(Lexer* lexer);
std::optional<TypeExpSyntax> ParseIdChainTypeExp(Lexer* lexer);

// std::optional<FuncTypeExpSyntax> ParseFuncTypeExp(Lexer* lexer);
// std::optional<TupleTypeExpSyntax> ParseTupleTypeExp(Lexer* lexer);

std::optional<LocalTypeExpSyntax> ParseLocalTypeExp(Lexer* lexer);
std::optional<TypeExpSyntax> ParseTypeExp(Lexer* lexer);
}