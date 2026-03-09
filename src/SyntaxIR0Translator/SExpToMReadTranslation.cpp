#include "SExpToMReadTranslation.h"

#include "Infra/Expected.h"
#include "SExpToReExpTranslation.h"
#include "ReExpToMReadTranslation.h"

using namespace std;

namespace Citron {

expected<MRead, DiagPtr> TranslateSExpToMRead(SExp* sExp, RType* hintType, TranslationContexts& contexts)
{
    auto e_reExp = TranslateSExpToReExp(sExp, hintType, contexts);
    RETURN_ON_ERROR(e_reExp);

    return TranslateReExpToMRead(*e_reExp, contexts);
}

} // namespace Citron