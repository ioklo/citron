#pragma once
#include <vector>
#include <memory>
#include <Symbol/MModuleDecl.h>

namespace Citron {

using MFuncDeclPtr = std::shared_ptr<class MFuncDecl>;
class RStmt;

// DeclSymbol -> Stmt
struct RStmtBody
{
    MFuncDeclPtr decl;
    std::vector<RStmt> stmts;
};

class RProgram
{
    std::shared_ptr<MModuleDecl> moduleDecl;
    std::vector<RStmtBody> bodies;
};

}

