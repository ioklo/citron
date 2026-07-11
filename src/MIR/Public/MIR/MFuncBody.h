#pragma once

namespace Citron {

struct MStmt_Scope;
class RFuncDecl;

struct MFuncBody
{
    RFuncDecl* nFuncDecl;
    MStmt_Scope* body;
};

} // namespace Citron
