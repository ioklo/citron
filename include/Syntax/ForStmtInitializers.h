#pragma once
#include "SyntaxConfig.h"
#include <variant>
#include "Exps.h"
#include "VarDecl.h"

namespace Citron::Syntax {

struct ExpForStmtInitializer
{
    Exp Exp;
};

struct VarDeclForStmtInitializer
{
    VarDecl VarDecl;
};

using ForStmtInitializer = std::variant<ExpForStmtInitializer, VarDeclForStmtInitializer>;

} // namespace Citron::Syntax

