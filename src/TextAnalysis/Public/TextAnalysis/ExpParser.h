#pragma once
#include "TextAnalysisConfig.h"

#include <optional>
#include <vector>

#include "Syntax/Syntax.h"

#include "Lexer.h"

namespace Citron {

class SFactory;

TEXTANALYSIS_API SExp* ParseExp(Lexer* lexer, SFactory& factory);

// 1. Assignment, Right Assoc
SExp* ParseAssignExp(Lexer* lexer, SFactory& factory);

// 2. Equality, Left Assoc
SExp* ParseEqualityExp(Lexer* lexer, SFactory& factory);

// 3. TestAndTypeTest, LeftAssoc
SExp* ParseTestAndTypeTestExp(Lexer* lexer, SFactory& factory);

// 4. Additive, LeftAssoc
SExp* ParseAdditiveExp(Lexer* lexer, SFactory& factory);

// 5. Multiplicative, LeftAssoc
SExp* ParseMultiplicativeExp(Lexer* lexer, SFactory& factory);

// 6. Unary, Prefix Inc / Dec
SExp* ParseUnaryExp(Lexer* lexer, SFactory& factory);

// 7. Primary, Postfix Inc / Dec
SExp* ParsePrimaryExp(Lexer* lexer, SFactory& factory);

// 8. Single
SExp* ParseSingleExp(Lexer* lexer, SFactory& factory);

// 기타
SExp_Box* ParseBoxExp(Lexer* lexer, SFactory& factory);
SExp_New* ParseNewExp(Lexer* lexer, SFactory& factory);

// LambdaExpression, Right Assoc
SExp_Lambda* ParseLambdaExp(Lexer* lexer, SFactory& factory);
SExp* ParseParenExp(Lexer* lexer, SFactory& factory);
SExp_NullLiteral* ParseNullLiteralExp(Lexer* lexer, SFactory& factory);
SExp_BoolLiteral* ParseBoolLiteralExp(Lexer* lexer, SFactory& factory);
SExp_IntLiteral* ParseIntLiteralExp(Lexer* lexer, SFactory& factory);
SExp_String* ParseStringExp(Lexer* lexer, SFactory& factory);
SExp_List* ParseListExp(Lexer* lexer, SFactory& factory);
SExp_Identifier* ParseIdentifierExp(Lexer* lexer, SFactory& factory);

SArguments* ParseCallArgs(Lexer* lexer, SFactory& factory);

}