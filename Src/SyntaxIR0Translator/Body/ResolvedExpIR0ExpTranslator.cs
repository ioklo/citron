using System.Diagnostics;
using Citron.Symbol;

using R = Citron.IR0;
using ISyntaxNode = Citron.Syntax.ISyntaxNode;

using static Citron.Analysis.SyntaxAnalysisErrorCode;

namespace Citron.Analysis
{
    // 기본적으로 load를 한다
    struct ResolvedExpIR0ExpTranslator : IResolvedExpVisitor<TranslationResult<R.Exp>>
    {
        ScopeContext context;
        ISyntaxNode nodeForErrorReport;

        public static TranslationResult<R.Exp> Translate(ResolvedExp reExp, ScopeContext context, ISyntaxNode nodeForErrorReport)
        {
            var translator = new ResolvedExpIR0ExpTranslator { context = context, nodeForErrorReport = nodeForErrorReport };
            return reExp.Accept<ResolvedExpIR0ExpTranslator, TranslationResult<R.Exp>>(ref translator);
        }

        TranslationResult<R.Exp> HandleLoc(R.Loc loc, IType type)
        {
            return TranslationResult.Valid<R.Exp>(new R.LoadExp(loc, type));
        }

        TranslationResult<R.Exp> HandleLocTranslationResult(TranslationResult<R.Loc> translationResult, IType type)
        {
            if (!translationResult.IsValid(out var loc))
                return TranslationResult.Error<R.Exp>();

            return HandleLoc(loc, type);
        }

        TranslationResult<R.Exp> IResolvedExpVisitor<TranslationResult<R.Exp>>.VisitClassMemberVar(ResolvedExp.ClassMemberVar reExp)
        {
            var locResult = new CoreResolvedExpIR0LocTranslator(context, nodeForErrorReport).TranslateClassMemberVar(reExp);
            return HandleLocTranslationResult(locResult, context.GetExpType(reExp));
        }

        TranslationResult<R.Exp> IResolvedExpVisitor<TranslationResult<R.Exp>>.VisitEnumElemMemberVar(ResolvedExp.EnumElemMemberVar reExp)
        {
            var locResult = new CoreResolvedExpIR0LocTranslator(context, nodeForErrorReport).TranslateEnumElemMemberVar(reExp);
            return HandleLocTranslationResult(locResult, reExp.Symbol.GetDeclType());
        }

        TranslationResult<R.Exp> IResolvedExpVisitor<TranslationResult<R.Exp>>.VisitIR0Exp(ResolvedExp.IR0Exp reExp)
        {
            return TranslationResult.Valid(reExp.Exp);
        }

        TranslationResult<R.Exp> IResolvedExpVisitor<TranslationResult<R.Exp>>.VisitLambdaMemberVar(ResolvedExp.LambdaMemberVar reExp)
        {
            var loc = new CoreResolvedExpIR0LocTranslator(context, nodeForErrorReport).TranslateLambdaMemberVar(reExp);
            return HandleLoc(loc, reExp.Symbol.GetDeclType());
        }

        TranslationResult<R.Exp> IResolvedExpVisitor<TranslationResult<R.Exp>>.VisitListIndexer(ResolvedExp.ListIndexer reExp)
        {
            var locResult = new CoreResolvedExpIR0LocTranslator(context, nodeForErrorReport).TranslateListIndexer(reExp);
            return HandleLocTranslationResult(locResult, reExp.ItemType);
        }

        TranslationResult<R.Exp> IResolvedExpVisitor<TranslationResult<R.Exp>>.VisitLocalVar(ResolvedExp.LocalVar reExp)
        {
            var loc = new CoreResolvedExpIR0LocTranslator(context, nodeForErrorReport).TranslateLocalVar(reExp);
            return HandleLoc(loc, reExp.Type);
        }

        TranslationResult<R.Exp> IResolvedExpVisitor<TranslationResult<R.Exp>>.VisitStructMemberVar(ResolvedExp.StructMemberVar reExp)
        {
            var locResult = new CoreResolvedExpIR0LocTranslator(context, nodeForErrorReport).TranslateStructMemberVar(reExp);
            return HandleLocTranslationResult(locResult, reExp.Symbol.GetDeclType());
        }

        TranslationResult<R.Exp> IResolvedExpVisitor<TranslationResult<R.Exp>>.VisitThis(ResolvedExp.ThisVar reExp)
        {
            var loc = new CoreResolvedExpIR0LocTranslator(context, nodeForErrorReport).TranslateThis(reExp);
            return HandleLoc(loc, reExp.Type);
        }

        TranslationResult<R.Exp> IResolvedExpVisitor<TranslationResult<R.Exp>>.VisitLocalDeref(ResolvedExp.LocalDeref reExp)
        {
            var loc = new CoreResolvedExpIR0LocTranslator(context, nodeForErrorReport).TranslateLocalDeref(reExp);
            return HandleLocTranslationResult(loc, context.GetExpType(reExp));
        }

        // *x
        TranslationResult<R.Exp> IResolvedExpVisitor<TranslationResult<R.Exp>>.VisitBoxDeref(ResolvedExp.BoxDeref reExp)
        {
            var loc = new CoreResolvedExpIR0LocTranslator(context, nodeForErrorReport).TranslateBoxDeref(reExp);
            return HandleLocTranslationResult(loc, context.GetExpType(reExp));
        }
    }
}