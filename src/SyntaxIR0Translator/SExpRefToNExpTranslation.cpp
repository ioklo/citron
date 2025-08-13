#include "SExpRefToNExpTranslation.h"

#include <expected>

#include "SExpRefToIrExpTranslation.h"
#include "IrExpToNExpTranslation.h"

#include "IrExp.h"

using namespace std;

namespace Citron::SyntaxIR0Translator {

expected<NExp*, DiagPtr> TranslateSExpRefToNExp(SExp& exp, TranslationContext& context)
{
    auto irExp = TranslateSExpRefToIrExp(exp, context);
    if (!irExp) return unexpected{move(irExp).error()};

    return TranslateIrExpToNExp(**irExp, context);
}

}
