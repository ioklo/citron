#include "pch.h"
#include "SExpRefToNExpTranslation.h"

#include "SExpRefToIrExpTranslation.h"
#include "IrExpToNExpTranslation.h"

#include "IrExp.h"

namespace Citron::SyntaxIR0Translator {

NExpPtr TranslateSExpRefToNExp(SExp& exp, TranslationContext& context)
{
    auto irExp = TranslateSExpRefToIrExp(exp, context);
    if (!irExp) return nullptr;

    return TranslateIrExpToNExp(*irExp, context);
}

}
