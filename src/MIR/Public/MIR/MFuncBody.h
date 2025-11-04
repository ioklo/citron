#pragma once
#include <vector>

namespace Citron {

class NFuncDecl;
class MStmt;

struct MFuncBody
{
    NFuncDecl* nFuncDecl;
    std::vector<MStmt*> stmts;
};

} // namespace Citron
