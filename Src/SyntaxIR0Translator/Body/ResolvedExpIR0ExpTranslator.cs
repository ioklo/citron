using System.Diagnostics;
using Citron.Symbol;
using R = Citron.IR0;
using ISyntaxNode = Citron.Syntax.ISyntaxNode;

namespace Citron.Analysis
{
    // 기본적으로 load를 한다
    struct ResolvedExpIR0ExpTranslator : IResolvedExpVisitor<TranslationResult<R.Exp>>
    {
        ScopeContext context;
        bool bDerefIfTypeIsRef;
        ISyntaxNode nodeForErrorReport;

        public static TranslationResult<R.Exp> Translate(ResolvedExp reExp, ScopeContext context, bool bDerefIfTypeIsRef, ISyntaxNode nodeForErrorReport)
        {
            var translator = new ResolvedExpIR0ExpTranslator { context = context, bDerefIfTypeIsRef = bDerefIfTypeIsRef, nodeForErrorReport = nodeForErrorReport };
            return reExp.Accept<ResolvedExpIR0ExpTranslator, TranslationResult<R.Exp>>(ref translator);
        }

        TranslationResult<R.Exp> HandleLoc(R.Loc loc, IType type)
        {
            if (bDerefIfTypeIsRef)
            {
                if (type is LocalRefType localReftype)
                    return TranslationResult.Valid<R.Exp>(new R.LoadExp(new R.LocalDerefLoc(loc), localReftype.InnerType));
                else if (type is BoxRefType boxRefType)
                    return TranslationResult.Valid<R.Exp>(new R.LoadExp(new R.BoxDerefLoc(loc), boxRefType.InnerType));
            }
            
            return TranslationResult.Valid<R.Exp>(new R.LoadExp(loc, type));
        }

        TranslationResult<R.Exp> HandleLocResult(TranslationResult<R.Loc> translationResult, IType type)
        {
            if (!translationResult.IsValid(out var loc))
                return TranslationResult.Error<R.Exp>();

            return HandleLoc(loc, type);
        }

        TranslationResult<R.Exp> IResolvedExpVisitor<TranslationResult<R.Exp>>.VisitClassMemberVar(ResolvedExp.ClassMemberVar reExp)
        {
            var locResult = new CoreResolvedExpIR0LocTranslator(context, nodeForErrorReport).TranslateClassMemberVar(reExp);
            return HandleLocResult(locResult, reExp.GetExpType());
        }

        TranslationResult<R.Exp> IResolvedExpVisitor<TranslationResult<R.Exp>>.VisitEnumElemMemberVar(ResolvedExp.EnumElemMemberVar reExp)
        {
            var locResult = new CoreResolvedExpIR0LocTranslator(context, nodeForErrorReport).TranslateEnumElemMemberVar(reExp);
            return HandleLocResult(locResult, reExp.Symbol.GetDeclType());
        }

        TranslationResult<R.Exp> IResolvedExpVisitor<TranslationResult<R.Exp>>.VisitIR0Exp(ResolvedExp.IR0Exp reExp)
        {
            if (bDerefIfTypeIsRef)
            {
                var expType = reExp.GetExpType();
                if (expType is LocalRefType localRefType)
                {
                    return TranslationResult.Valid<R.Exp>(new R.LoadExp(new R.LocalDerefLoc(new R.TempLoc(reExp.Exp)), localRefType.InnerType));
                }
                else if (expType is BoxRefType boxRefType)
                {
                    return TranslationResult.Valid<R.Exp>(new R.LoadExp(new R.BoxDerefLoc(new R.TempLoc(reExp.Exp)), boxRefType.InnerType));
                }
            }

            return TranslationResult.Valid(reExp.Exp);
        }

        TranslationResult<R.Exp> IResolvedExpVisitor<TranslationResult<R.Exp>>.VisitLambdaMemberVar(ResolvedExp.LambdaMemberVar reExp)
        {
            var loc = new CoreResolvedExpIR0LocTranslator(context, nodeForErrorReport).TranslateLambdaMemberVar(reExp);
            return HandleLoc(loc, reExp.Symbol.GetDeclType());
        }

        TranslationResult<R.Exp> IResolvedExpVisitor<TranslationResult<R.Exp>>.VisitListIndexer(ResolvedExp.ListIndexer reExp)
        {
            var loc = new CoreResolvedExpIR0LocTranslator(context, nodeForErrorReport).TranslateListIndexer(reExp);
            return HandleLoc(loc, reExp.ItemType);
        }

        TranslationResult<R.Exp> IResolvedExpVisitor<TranslationResult<R.Exp>>.VisitLocalDeref(ResolvedExp.LocalDeref reExp)
        {
            var locResult = new CoreResolvedExpIR0LocTranslator(context, nodeForErrorReport).TranslateLocalDeref(reExp);
            return HandleLocResult(locResult, reExp.Type);
        }

        TranslationResult<R.Exp> IResolvedExpVisitor<TranslationResult<R.Exp>>.VisitBoxDeref(ResolvedExp.BoxDeref reExp)
        {
            var locResult = new CoreResolvedExpIR0LocTranslator(context, nodeForErrorReport).TranslateBoxDeref(reExp);
            return HandleLocResult(locResult, reExp.Type);
        }

        TranslationResult<R.Exp> IResolvedExpVisitor<TranslationResult<R.Exp>>.VisitLocalVar(ResolvedExp.LocalVar reExp)
        {
            var loc = new CoreResolvedExpIR0LocTranslator(context, nodeForErrorReport).TranslateLocalVar(reExp);
            return HandleLoc(loc, reExp.Type);
        }

        TranslationResult<R.Exp> IResolvedExpVisitor<TranslationResult<R.Exp>>.VisitStructMemberVar(ResolvedExp.StructMemberVar reExp)
        {
            var locResult = new CoreResolvedExpIR0LocTranslator(context, nodeForErrorReport).TranslateStructMemberVar(reExp);
            return HandleLocResult(locResult, reExp.Symbol.GetDeclType());
        }

        TranslationResult<R.Exp> IResolvedExpVisitor<TranslationResult<R.Exp>>.VisitThis(ResolvedExp.ThisVar reExp)
        {
            var loc = new CoreResolvedExpIR0LocTranslator(context, nodeForErrorReport).TranslateThis(reExp);
            return HandleLoc(loc, reExp.Type);
        }
    }
}