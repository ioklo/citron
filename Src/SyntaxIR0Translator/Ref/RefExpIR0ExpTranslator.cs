using S = Citron.Syntax;
using R = Citron.IR0;
using Citron.Symbol;

namespace Citron.Analysis;

// '&' S.Exp -> R.BoxPtrExp, R.LocalPtrExp
struct RefExpIR0ExpTranslator
{
    public static TranslationResult<R.Exp> Translate(S.Exp exp, ScopeContext context)
    {
        var imRefExpResult = RefExpIntermediateRefExpTranslator.Translate(exp, context);
        if (!imRefExpResult.IsValid(out var imRefExp))
            return TranslationResult.Error<R.Exp>();

        return IntermediateRefExpIR0ExpTranslator.Translate(imRefExp, context, exp);
    }
}

