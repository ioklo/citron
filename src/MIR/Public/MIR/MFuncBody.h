#pragma once
#include <vector>

namespace Citron {

class NFuncDecl;
class MStmt;

struct MFuncBody
{
    NFuncDecl* funcDecl;
    std::vector<MStmt*> stmts;
};

} // namespace Citron
