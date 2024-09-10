#include "pch.h"
#include "StmtTranslation.h"

#include <IR0/RStmt.h>
#include <Syntax/Syntax.h>

using namespace std;

namespace Citron {

optional<vector<Citron::RStmtPtr>> TranslateBody(vector<SStmtPtr>& stmts, ScopeContext& context)
{
    static_assert(false);
    return nullopt;
}

}
