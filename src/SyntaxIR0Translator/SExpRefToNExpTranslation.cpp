module Citron.SyntaxIR0Translator:SExpRefToNExpTranslation;

import :SExpRefToIrExpTranslation;
import :IrExpToNExpTranslation;

import :IrExp;

namespace Citron::SyntaxIR0Translator {

NExpPtr TranslateSExpRefToNExp(SExp& exp, TranslationContext& context)
{
    auto irExp = TranslateSExpRefToIrExp(exp, context);
    if (!irExp) return nullptr;

    return TranslateIrExpToNExp(*irExp, context);
}

}
