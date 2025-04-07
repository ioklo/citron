module Citron.SyntaxIR0Translator:SExpRefToNExpTranslation;

import <expected>;

import :SExpRefToIrExpTranslation;
import :IrExpToNExpTranslation;

import :IrExp;

using namespace std;

namespace Citron::SyntaxIR0Translator {

expected<NExpPtr, DiagPtr> TranslateSExpRefToNExp(SExp& exp, TranslationContext& context)
{
    auto irExp = TranslateSExpRefToIrExp(exp, context);
    if (!irExp) return nullptr;

    return TranslateIrExpToNExp(**irExp, context);
}

}
