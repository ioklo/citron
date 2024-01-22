#pragma once
#include "SyntaxConfig.h"
#include <variant>
#include "ExpSyntaxes.h"
#include "VarDeclSyntax.h"

namespace Citron {

struct ExpForStmtInitializerSyntax
{
    ExpSyntax Exp;
};

struct VarDeclForStmtInitializerSyntax
{
    VarDeclSyntax VarDecl;
};

using ForStmtInitializerSyntax = std::variant<ExpForStmtInitializerSyntax, VarDeclForStmtInitializerSyntax>;

} // namespace Citron

