#pragma once
#include "NSymbol/NFuncDecl.h"

namespace Citron {

struct MStmt_Scope;

struct MFuncBody
{
    NFuncDecl nFuncDecl;
    MStmt_Scope* body;
};

} // namespace Citron
