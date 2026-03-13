#include "SExpToReExp.h"

#include "Infra/Expected.h"
#include "SExpToImExp.h"
#include "ImExpToReExp.h"

using namespace std;

namespace Citron {

expected<ReExp, DiagPtr> TranslateSExpToReExp(SExp* exp, RType* hintType, TranslationContexts& contexts)
{
    auto e_imExp = TranslateSExpToImExp(exp, hintType, contexts);
    RETURN_ON_ERROR(e_imExp);

    return TranslateImExpToReExp(*e_imExp, contexts);
}

} // namespace Citron