#include "SExpToMExpTranslation.h"

#include <variant>
#include <cassert>

#include "Infra/Ptr.h"
#include "Infra/Exceptions.h"
#include "Infra/Unreachable.h"
#include "Infra/Expected.h"
#include "Logging/Logger.h"
#include "Syntax/Syntax.h"
#include "RSymbol/RTypes.h"
#include "RSymbol/RFactory.h"
#include "MIR/MLoc.h"
#include "MIR/MExp.h"
#include "MIR/MFactory.h"

#include "ReExp.h"
#include "ImExp.h"

#include "SExpToMLocTranslation.h"
#include "SExpToReExpTranslation.h"
#include "SExpToImExp.h"
#include "ImExpAndSArgsToReExpTranslation.h"
#include "ScopeContext.h"
#include "Misc.h"
#include "BinOpQueryService.h"
#include "TranslationContexts.h"
#include "DesignatedDiagnostic.h"

using namespace std;

namespace Citron {



expected<MExp*, DiagPtr> TranslateSExpToMExp(SExp* exp, RType* hintType, TranslationContexts& contexts)
{
    auto e_reExp = TranslateSExpToReExp(exp, hintType, contexts);
    RETURN_ON_ERROR(e_reExp);

    return TranslateReExpToMExp(*e_reExp, contexts);
}

}