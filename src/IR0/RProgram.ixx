export module Citron.RProgram;

import <vector>;

namespace Citron {

// DeclSymbol -> Stmt
struct StmtBody
{
    MDecl decl;
    std::vector<Stmt> stmts;
};

class RProgram
{
    MModuleDecl moduleDecl;
    std::vector<StmtBody> bodies;
}

}

