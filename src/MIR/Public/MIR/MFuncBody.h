#pragma once
#include <vector>

namespace Citron {

class NFuncDecl;
struct MStmt;

struct MFuncBody
{
    NFuncDecl* nFuncDecl;
    std::vector<MStmt*> stmts;
};

} // namespace Citron
