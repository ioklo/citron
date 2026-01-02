#include "SExpRefToMExpTranslation.h"

#include <expected>

#include "Infra/Expected.h"

#include "SExpRefToIrExpTranslation.h"
#include "IrExpToMExpTranslation.h"

#include "IrExp.h"
#include "TranslationContexts.h"

using namespace std;

namespace Citron {

expected<MExp*, DiagPtr> TranslateSExpRefToMExp(SExp* exp, TranslationContexts& contexts)
{
    auto e_irExp = TranslateSExpRefToIrExp(exp, contexts);
    RETURN_ON_ERROR(e_irExp);

    return TranslateIrExpToMExp(*e_irExp, contexts);
}

}
