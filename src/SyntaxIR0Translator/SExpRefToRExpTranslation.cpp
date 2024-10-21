#include "pch.h"
#include "SExpRefToRExpTranslation.h"

#include "SExpRefToIrExpTranslation.h"
#include "IrExpToRExpTranslation.h"

#include "IrExp.h"

namespace Citron::SyntaxIR0Translator {

RExpPtr TranslateSExpRefToRExp(SExp& exp, TranslationContext& context)
{
    auto irExp = TranslateSExpRefToIrExp(exp, context);
    if (!irExp) return nullptr;

    return TranslateIrExpToRExp(*irExp, context);
}

}
