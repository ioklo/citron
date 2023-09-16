using System.Diagnostics;
using Citron.Symbol;

using R = Citron.IR0;
using ISyntaxNode = Citron.Syntax.ISyntaxNode;

using static Citron.Analysis.SyntaxAnalysisErrorCode;

namespace Citron.Analysis
{
    // 기본적으로 load를 한다
    struct ResolvedExpIR0ExpTranslator : IResolvedExpVisitor<TranslationResult<IR0ExpResult>>
    {
        ScopeContext context;
        ISyntaxNode nodeForErrorReport;

        public static TranslationResult<IR0ExpResult> Translate(ResolvedExp reExp, ScopeContext context, ISyntaxNode nodeForErrorReport)
        {
            var translator = new ResolvedExpIR0ExpTranslator { context = context, nodeForErrorReport = nodeForErrorReport };
            return reExp.Accept<ResolvedExpIR0ExpTranslator, TranslationResult<IR0ExpResult>>(ref translator);
        }

        TranslationResult<IR0ExpResult> HandleLoc(R.Loc loc, IType type)
        {
            return TranslationResult.Valid<IR0ExpResult>(new IR0ExpResult(new R.LoadExp(loc, type), type));
        }

        TranslationResult<IR0ExpResult> HandleLocTranslationResult(TranslationResult<R.Loc> translationResult, IType type)
        {
            if (!translationResult.IsValid(out var loc))
                return TranslationResult.Error<IR0ExpResult>();

            return HandleLoc(loc, type);
        }

        TranslationResult<IR0ExpResult> IResolvedExpVisitor<TranslationResult<IR0ExpResult>>.VisitClassMemberVar(ResolvedExp.ClassMemberVar reExp)
        {
            var locResult = new CoreResolvedExpIR0LocTranslator(context, nodeForErrorReport).TranslateClassMemberVar(reExp);
            return HandleLocTranslationResult(locResult, reExp.GetExpType());
        }

        TranslationResult<IR0ExpResult> IResolvedExpVisitor<TranslationResult<IR0ExpResult>>.VisitEnumElemMemberVar(ResolvedExp.EnumElemMemberVar reExp)
        {
            var locResult = new CoreResolvedExpIR0LocTranslator(context, nodeForErrorReport).TranslateEnumElemMemberVar(reExp);
            return HandleLocTranslationResult(locResult, reExp.Symbol.GetDeclType());
        }

        TranslationResult<IR0ExpResult> IResolvedExpVisitor<TranslationResult<IR0ExpResult>>.VisitIR0Exp(ResolvedExp.IR0Exp reExp)
        {
            return TranslationResult.Valid(reExp.ExpResult);
        }

        TranslationResult<IR0ExpResult> IResolvedExpVisitor<TranslationResult<IR0ExpResult>>.VisitLambdaMemberVar(ResolvedExp.LambdaMemberVar reExp)
        {
            var loc = new CoreResolvedExpIR0LocTranslator(context, nodeForErrorReport).TranslateLambdaMemberVar(reExp);
            return HandleLoc(loc, reExp.Symbol.GetDeclType());
        }

        TranslationResult<IR0ExpResult> IResolvedExpVisitor<TranslationResult<IR0ExpResult>>.VisitListIndexer(ResolvedExp.ListIndexer reExp)
        {
            var locResult = new CoreResolvedExpIR0LocTranslator(context, nodeForErrorReport).TranslateListIndexer(reExp);
            return HandleLocTranslationResult(locResult, reExp.ItemType);
        }

        TranslationResult<IR0ExpResult> IResolvedExpVisitor<TranslationResult<IR0ExpResult>>.VisitLocalVar(ResolvedExp.LocalVar reExp)
        {
            var loc = new CoreResolvedExpIR0LocTranslator(context, nodeForErrorReport).TranslateLocalVar(reExp);
            return HandleLoc(loc, reExp.Type);
        }

        TranslationResult<IR0ExpResult> IResolvedExpVisitor<TranslationResult<IR0ExpResult>>.VisitStructMemberVar(ResolvedExp.StructMemberVar reExp)
        {
            var locResult = new CoreResolvedExpIR0LocTranslator(context, nodeForErrorReport).TranslateStructMemberVar(reExp);
            return HandleLocTranslationResult(locResult, reExp.Symbol.GetDeclType());
        }

        TranslationResult<IR0ExpResult> IResolvedExpVisitor<TranslationResult<IR0ExpResult>>.VisitThis(ResolvedExp.ThisVar reExp)
        {
            var loc = new CoreResolvedExpIR0LocTranslator(context, nodeForErrorReport).TranslateThis(reExp);
            return HandleLoc(loc, reExp.Type);
        }

        TranslationResult<IR0ExpResult> IResolvedExpVisitor<TranslationResult<IR0ExpResult>>.VisitLocalDeref(ResolvedExp.LocalDeref reExp)
        {
            var loc = new CoreResolvedExpIR0LocTranslator(context, nodeForErrorReport).TranslateLocalDeref(reExp);
            return HandleLocTranslationResult(loc, reExp.GetExpType());
        }

        // *x
        TranslationResult<IR0ExpResult> IResolvedExpVisitor<TranslationResult<IR0ExpResult>>.VisitBoxDeref(ResolvedExp.BoxDeref reExp)
        {
            var loc = new CoreResolvedExpIR0LocTranslator(context, nodeForErrorReport).TranslateBoxDeref(reExp);
            return HandleLocTranslationResult(loc, reExp.GetExpType());
        }
    }
}