#include "Translations.h"

#include "Infra/Expected.h"
#include "SExpToReExp.h"
#include "ReExpToMIR.h"
#include "SExpToIrExp.h"
#include "IrExpToMSharedExp.h"


using namespace std;

namespace Citron {

expected<MCreate, DiagPtr> TranslateSExpToMCreate(SExp* sExp, RType* hintType, TranslationContexts& contexts)
{
    auto e_reExp = TranslateSExpToReExp(sExp, hintType, contexts);
    RETURN_ON_ERROR(e_reExp);

    return TranslateReExpToMCreate(*e_reExp, contexts);
}

expected<MRead, DiagPtr> TranslateSExpToMRead(SExp* sExp, RType* hintType, TranslationContexts& contexts)
{
    auto e_reExp = TranslateSExpToReExp(sExp, hintType, contexts);
    RETURN_ON_ERROR(e_reExp);

    return TranslateReExpToMRead(*e_reExp, contexts);
}

expected<MLoc*, DiagPtr> TranslateSExpToMLoc(SExp* sExp, RType* hintType, bool bMaterializeExp, IDesignatedDiagnostic* notLocationDiag, TranslationContexts& contexts)
{
    auto e_reExp = TranslateSExpToReExp(sExp, hintType, contexts);
    RETURN_ON_ERROR(e_reExp);

    return TranslateReExpToMLoc(*e_reExp, bMaterializeExp, notLocationDiag, contexts);
}

expected<MSharedExp*, DiagPtr> TranslateSExpToMSharedExp(SExp* sExp, TranslationContexts& contexts)
{
    auto e_irExp = TranslateSExpToIrExp(sExp, contexts);
    RETURN_ON_ERROR(e_irExp);

    return TranslateIrExpToMSharedExp(*e_irExp, contexts);
}

} // namespace Citron