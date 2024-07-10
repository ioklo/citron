#include <vector>

namespace Citron {

// DeclSymbol -> Stmt
struct RStmtBody
{
    MFuncDecl decl;
    std::vector<RStmt> stmts;
};

class RProgram
{
    MModuleDecl moduleDecl;
    std::vector<RStmtBody> bodies;
}

}

