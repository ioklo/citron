using S = Citron.Syntax;
using R = Citron.IR0;

namespace Citron.Analysis;

// box var& x = <exp> 꼴의 표현
// <exp>의 결과는 box를 만드는 exp
// S.Exp -> R.Exp
struct ExpIR0BoxRefTranslator
{
    public static TranslationResult<R.Exp> Translate(S.Exp exp, ScopeContext context, S.ISyntaxNode nodeForErrorReport)
    {
        var reExpResult = ExpResolvedExpTranslator.Translate(exp, context, hintType: null);
        if (!reExpResult.IsValid(out var reExp))
            return TranslationResult.Error<R.Exp>();

        return ResolvedExpIR0BoxRefTranslator.Translate(reExp, context, nodeForErrorReport);
    }
}
