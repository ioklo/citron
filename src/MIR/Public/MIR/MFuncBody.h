#pragma once

namespace Citron {

struct MStmt_Scope;
class RFuncDecl;

struct MFuncBody
{
    RFuncDecl* rFuncDecl;
    MStmt_Scope* body;
};

} // namespace Citron
