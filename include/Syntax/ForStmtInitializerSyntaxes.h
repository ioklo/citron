#pragma once
#include "SyntaxConfig.h"
#include <variant>
#include <Infra/Json.h>

#include "ExpSyntaxes.h"
#include "VarDeclSyntax.h"


namespace Citron {

struct ExpForStmtInitializerSyntax
{
    ExpSyntax exp;
};

BEGIN_IMPLEMENT_JSON_STRUCT_INLINE(ExpForStmtInitializerSyntax, syntax)
    IMPLEMENT_JSON_MEMBER_DIRECT(syntax, exp)
END_IMPLEMENT_JSON_STRUCT_INLINE()

struct VarDeclForStmtInitializerSyntax
{
    VarDeclSyntax varDecl;
};

BEGIN_IMPLEMENT_JSON_STRUCT_INLINE(VarDeclForStmtInitializerSyntax, syntax)
    IMPLEMENT_JSON_MEMBER_DIRECT(syntax, varDecl)
END_IMPLEMENT_JSON_STRUCT_INLINE()

using ForStmtInitializerSyntax = std::variant<ExpForStmtInitializerSyntax, VarDeclForStmtInitializerSyntax>;

inline JsonItem ToJson(ForStmtInitializerSyntax& syntax)
{
    return std::visit([](auto&& init) { return ToJson(init); }, syntax);
}

} // namespace Citron

