#include "SExpRefToNExpTranslation.h"

#include <expected>

#include "SExpRefToIrExpTranslation.h"
#include "IrExpToNExpTranslation.h"

#include "IrExp.h"

using namespace std;

namespace Citron::SyntaxIR0Translator {

expected<NExp*, DiagPtr> TranslateSExpRefToNExp(SExp* exp, TranslationContext& context)
{
    auto eIrExp = TranslateSExpRefToIrExp(exp, context);
    if (!eIrExp) return unexpected{move(eIrExp).error()};

    return TranslateIrExpToNExp(*eIrExp, context);
}

}
