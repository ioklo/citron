using Citron.IR0;
using Citron.Symbol;
using ISyntaxNode = Citron.Syntax.ISyntaxNode;
using static Citron.Analysis.SyntaxAnalysisErrorCode;

namespace Citron.Analysis;

// struct의 instance를
struct ResolvedInstanceExpStructMemberVarBinder : IResolvedInstanceExpVisitor<TranslationResult<Exp>>
{
    ScopeContext context;
    ISyntaxNode nodeForErrorReport;
    StructMemberVarSymbol symbol;

    public static TranslationResult<Exp> Bind(ResolvedInstanceExp reInExp, ScopeContext context, ISyntaxNode nodeForErrorReport, StructMemberVarSymbol symbol)
    {
        var binder = new ResolvedInstanceExpStructMemberVarBinder() { context = context, nodeForErrorReport = nodeForErrorReport, symbol = symbol };
        return reInExp.Accept<ResolvedInstanceExpStructMemberVarBinder, TranslationResult<Exp>>(ref binder);
    }

    static TranslationResult<Exp> Error()
    {
        return TranslationResult.Error<Exp>();
    }

    static TranslationResult<Exp> Valid(Exp exp)
    {
        return TranslationResult.Valid(exp);
    }

    TranslationResult<Exp> IResolvedInstanceExpVisitor<TranslationResult<Exp>>.VisitNormal(ResolvedInstanceExp.Normal reInExp)
    {
        context.AddFatalError(A3103_BoxRef_CantMakeBoxRefWithLocal, nodeForErrorReport);
        return Error();
    }

    TranslationResult<Exp> IResolvedInstanceExpVisitor<TranslationResult<Exp>>.VisitLocalDeref(ResolvedInstanceExp.LocalDeref reInExp)
    {
        // var s = S();
        // var& ps = s;
        // box var& x = ps.a; // error
        context.AddFatalError(A3101_BoxRef_CantMakeBoxRefWithLocalRef, nodeForErrorReport);
        return Error();
    }

    TranslationResult<Exp> IResolvedInstanceExpVisitor<TranslationResult<Exp>>.VisitBoxDeref(ResolvedInstanceExp.BoxDeref reInExp)
    {
        // box var& ps = box S();
        // box var& x = ps.a; // valid

        // deref를 하지 않고 box된 것을 그대로 사용한다
        var innerExpResult = ResolvedExpIR0ExpTranslator.Translate(reInExp.InnerExp, context, bDerefIfTypeIsRef: false, nodeForErrorReport);
        if (!innerExpResult.IsValid(out var innerExp))
            return Error();

        return Valid(new StructMemberVarBoxRefExp(innerExp, symbol));
    }
}
