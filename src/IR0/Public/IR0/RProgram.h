#pragma once
#include <vector>
#include <memory>
#include "RModuleDecl.h"

namespace Citron {

using RFuncDeclPtr = std::shared_ptr<class RFuncDecl>;
class RStmt;

// DeclSymbol -> Stmt
struct RStmtBody
{
    RFuncDeclPtr decl;
    std::vector<RStmt> stmts;
};

class RProgram
{
    std::shared_ptr<RModuleDecl> moduleDecl;
    std::vector<RStmtBody> bodies;
};

}

