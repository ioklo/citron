#include "SExpToMOperandTranslation.h"

#include "Infra/Expected.h"
#include "Infra/Exceptions.h"
#include "SExpToReExpTranslation.h"
#include "ReExpToMOperandTranslation.h"
#include "SExpToMExpTranslation.h"

#include "MIR/MExp.h"

using namespace std;

namespace Citron {

expected<MRead, DiagPtr> TranslateSExpToMOperand(SExp* sExp, RType* hintType, TranslationContexts& contexts)
{
    auto e_reExp = TranslateSExpToReExp(sExp, hintType, contexts);
    RETURN_ON_ERROR(e_reExp);

    return TranslateReExpToMOperand(*e_reExp, contexts);
}

} // namespace Citron