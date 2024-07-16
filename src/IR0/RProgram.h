#pragma once
#include <vector>

namespace Citron {

using MModuleDeclPtr = std::shared_ptr<class MModuleDecl>;
using MFuncDeclPtr = std::shared_ptr<class MFuncDecl>;

// DeclSymbol -> Stmt
struct RStmtBody
{
    MFuncDeclPtr decl;
    std::vector<RStmt> stmts;
};

class RProgram
{
    MModuleDeclPtr moduleDecl;
    std::vector<RStmtBody> bodies;
};

}

