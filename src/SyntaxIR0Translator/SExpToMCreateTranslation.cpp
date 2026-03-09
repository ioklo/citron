#include "SExpToMCreateTranslation.h"
#include "Infra/Expected.h"
#include "SExpToReExpTranslation.h"
#include "ReExpToMCreateTranslation.h"

using namespace std;

namespace Citron {

expected<MCreate, DiagPtr> TranslateSExpToMCreate(SExp* sExp, RType* hintType, TranslationContexts& contexts)
{
    auto e_reExp = TranslateSExpToReExp(sExp, hintType, contexts);
    RETURN_ON_ERROR(e_reExp);

    return TranslateReExpToMCreate(*e_reExp, contexts);
}

} // namespace Citron