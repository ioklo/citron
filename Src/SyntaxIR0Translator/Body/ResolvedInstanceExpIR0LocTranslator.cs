using ISyntaxNode = Citron.Syntax.ISyntaxNode;
using static Citron.Analysis.SyntaxAnalysisErrorCode;
using Citron.IR0;

namespace Citron.Analysis;

struct ResolvedInstanceExpIR0LocTranslator : IResolvedInstanceExpVisitor<TranslationResult<IR0LocResult>>
{
    ScopeContext context;
    bool bWrapExpAsLoc;
    ISyntaxNode nodeForErrorReport;
    SyntaxAnalysisErrorCode notLocationErrorCode;

    public static TranslationResult<IR0LocResult> Translate(
        ResolvedInstanceExp reInExp,
        ScopeContext context,
        bool bWrapExpAsLoc,
        ISyntaxNode nodeForErrorReport,
        SyntaxAnalysisErrorCode notLocationErrorCode) // default는 A2015_ResolveIdentifier_ExpressionIsNotLocation
    {
        var translator = new ResolvedInstanceExpIR0LocTranslator
        {
            context = context,
            bWrapExpAsLoc = bWrapExpAsLoc,
            nodeForErrorReport = nodeForErrorReport,
            notLocationErrorCode = notLocationErrorCode
        };
        return reInExp.Accept<ResolvedInstanceExpIR0LocTranslator, TranslationResult<IR0LocResult>>(ref translator);
    }

    static TranslationResult<IR0LocResult> Error() { return TranslationResult.Error<IR0LocResult>(); }
    static TranslationResult<IR0LocResult> Valid(IR0LocResult value) { return TranslationResult.Valid(value); }

    TranslationResult<IR0LocResult> IResolvedInstanceExpVisitor<TranslationResult<IR0LocResult>>.VisitNormal(ResolvedInstanceExp.Normal reInExp)
    {
        // forward, deref는 이미 처리되었기 때문에 따로 하지 않는다
        return ResolvedExpIR0LocTranslator.Translate(reInExp.Exp, context, bWrapExpAsLoc, bDerefIfTypeIsRef: false, nodeForErrorReport, notLocationErrorCode);
    }

    TranslationResult<IR0LocResult> IResolvedInstanceExpVisitor<TranslationResult<IR0LocResult>>.VisitBoxDeref(ResolvedInstanceExp.BoxDeref reInExp)
    {
        // bDerefIfTypeIsRef: false, innerLoc까지 deref를 해줄필요는 없다
        var innerResult = ResolvedExpIR0LocTranslator.Translate(reInExp.InnerExp, context, bWrapExpAsLoc: false, bDerefIfTypeIsRef: false, nodeForErrorReport, A2015_ResolveIdentifier_ExpressionIsNotLocation);
        if (!innerResult.IsValid(out var inner))
            return Error();

        return Valid(new IR0LocResult(new BoxDerefLoc(inner.Loc), inner.LocType));
    }

    TranslationResult<IR0LocResult> IResolvedInstanceExpVisitor<TranslationResult<IR0LocResult>>.VisitLocalDeref(ResolvedInstanceExp.LocalDeref reInExp)
    {
        // bDerefIfTypeIsRef: false, innerLoc까지 deref를 해줄필요는 없다
        var innerResult = ResolvedExpIR0LocTranslator.Translate(reInExp.InnerExp, context, bWrapExpAsLoc: false, bDerefIfTypeIsRef: false, nodeForErrorReport, A2015_ResolveIdentifier_ExpressionIsNotLocation);
        if (!innerResult.IsValid(out var inner))
            return Error();

        return Valid(new IR0LocResult(new LocalDerefLoc(inner.Loc), inner.LocType));
    }
    
}
    

