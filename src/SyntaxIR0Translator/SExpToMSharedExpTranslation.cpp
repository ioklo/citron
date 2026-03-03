#include "SExpToMSharedExpTranslation.h"

#include "Infra/Expected.h"
#include "Syntax/Syntax.h"
#include "Logging/Diag.h"
#include "RSymbol/RFactory.h"

#include "TranslationContexts.h"
#include "Misc.h"
#include "SExpToIrExpTranslation.h"
#include "IrExpAndMemberNameToMSharedExpTranslation.h"

using namespace std;

namespace Citron {

expected<MSharedExp*, DiagPtr> TranslateSExpToMSharedExp(SExp* sExp, TranslationContexts& contexts)
{
    auto e_irExp = TranslateSExpToIrExp(sExp, contexts);
    RETURN_ON_ERROR(e_irExp);

    return TranslateIrExpToMSharedExp(*e_irExp, contexts);
}

} // namespace Citron