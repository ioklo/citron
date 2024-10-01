#include "pch.h"
#include "StmtTranslation.h"

#include <optional>

#include <IR0/RStmt.h>
#include <Syntax/Syntax.h>
#include "ScopeContext.h"

using namespace std;

namespace Citron::SyntaxIR0Translator {

optional<vector<Citron::RStmtPtr>> TranslateBody(vector<SStmtPtr>& stmts, ScopeContextPtr context)
{
    static_assert(false);
    return nullopt;
}

}
