#pragma once
#include <vector>

namespace Citron {

class NFuncDecl;
struct MStmt_Scope;

struct MFuncBody
{
    NFuncDecl* nFuncDecl;
    MStmt_Scope* body;
};

} // namespace Citron
