#pragma once

#include <memory>
#include <optional>

#include "RSymbol/RNames.h"

namespace Citron {

class RType;
class RFactory;
struct MExp;
struct MLoc;
struct MInitExp;
struct ReExpVisitor;
struct MStmt_Call;
struct MStmt_Assign;

// ResolvedExp 
struct ReExp_Loc { MLoc* mLoc; };
struct ReExp_Exp { MExp* mExp; };
struct ReExp_InitExp { MInitExp* mInitExp; };
struct ReExp_StmtCall { MStmt_Call* mCallStmt; };
struct ReExp_StmtAssign { MStmt_Assign* mAssignStmt; };

using ReExp = std::variant<ReExp_Loc, ReExp_Exp, ReExp_InitExp, ReExp_StmtCall, ReExp_StmtAssign>;

RType* GetType(ReExp& reExp, RFactory* rFactory);

} // namespace Citron