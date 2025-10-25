#include "SExpRefToMExpTranslation.h"

#include <expected>

#include "SExpRefToIrExpTranslation.h"
#include "IrExpToMExpTranslation.h"

#include "IrExp.h"

using namespace std;

namespace Citron::SyntaxIR0Translator {

expected<MExp*, DiagPtr> TranslateSExpRefToMExp(SExp* exp, TranslationContext& context)
{
    auto eIrExp = TranslateSExpRefToIrExp(exp, context);
    if (!eIrExp) return unexpected{move(eIrExp).error()};

    return TranslateIrExpToMExp(*eIrExp, context);
}

}
