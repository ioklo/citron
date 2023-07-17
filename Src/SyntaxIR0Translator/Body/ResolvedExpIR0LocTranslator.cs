using System;
using Citron.Symbol;
using Pretune;

using S = Citron.Syntax;
using R = Citron.IR0;

using static Citron.Analysis.SyntaxAnalysisErrorCode;
using System.Diagnostics;

namespace Citron.Analysis;

// nothrow
struct ResolvedExpIR0LocTranslator : IResolvedExpVisitor<TranslationResult<IR0LocResult>>
{   
    ScopeContext context;    
    bool bWrapExpAsLoc;
    S.ISyntaxNode nodeForErrorReport;
    SyntaxAnalysisErrorCode notLocationErrorCode;

    public static TranslationResult<IR0LocResult> Translate(
        ResolvedExp reExp, 
        ScopeContext context, 
        bool bWrapExpAsLoc, 
        S.ISyntaxNode nodeForErrorReport,
        SyntaxAnalysisErrorCode notLocationErrorCode) // default는 A2015_ResolveIdentifier_ExpressionIsNotLocation
    {
        var translator = new ResolvedExpIR0LocTranslator {
            context = context,
            bWrapExpAsLoc = bWrapExpAsLoc,
            nodeForErrorReport = nodeForErrorReport,
            notLocationErrorCode = notLocationErrorCode
        };
        return reExp.Accept<ResolvedExpIR0LocTranslator, TranslationResult<IR0LocResult>>(ref translator);
    }

    TranslationResult<IR0LocResult> HandleLoc(R.Loc loc, IType type)
    {   
        return TranslationResult.Valid(new IR0LocResult(loc, type));
    }

    TranslationResult<IR0LocResult> HandleLocTranslationResult(TranslationResult<R.Loc> result, IType type)
    {
        if (!result.IsValid(out var loc))
            return TranslationResult.Error<IR0LocResult>();

        return HandleLoc(loc, type);
    }

    TranslationResult<IR0LocResult> IResolvedExpVisitor<TranslationResult<IR0LocResult>>.VisitThis(ResolvedExp.ThisVar reExp)
    {     
        return HandleLoc(new CoreResolvedExpIR0LocTranslator(context, nodeForErrorReport).TranslateThis(reExp), reExp.GetExpType());
    }

    TranslationResult<IR0LocResult> IResolvedExpVisitor<TranslationResult<IR0LocResult>>.VisitLocalVar(ResolvedExp.LocalVar reExp)
    {
        return HandleLoc(new CoreResolvedExpIR0LocTranslator(context, nodeForErrorReport).TranslateLocalVar(reExp), reExp.GetExpType());
    }

    TranslationResult<IR0LocResult> IResolvedExpVisitor<TranslationResult<IR0LocResult>>.VisitLambdaMemberVar(ResolvedExp.LambdaMemberVar reExp)
    {   
        return HandleLoc(new CoreResolvedExpIR0LocTranslator(context, nodeForErrorReport).TranslateLambdaMemberVar(reExp), reExp.GetExpType());
    }
    
    TranslationResult<IR0LocResult> IResolvedExpVisitor<TranslationResult<IR0LocResult>>.VisitClassMemberVar(ResolvedExp.ClassMemberVar reExp)
    {
        return HandleLocTranslationResult(new CoreResolvedExpIR0LocTranslator(context, nodeForErrorReport).TranslateClassMemberVar(reExp), reExp.GetExpType());
    }
    
    TranslationResult<IR0LocResult> IResolvedExpVisitor<TranslationResult<IR0LocResult>>.VisitStructMemberVar(ResolvedExp.StructMemberVar reExp)
    {
        return HandleLocTranslationResult(new CoreResolvedExpIR0LocTranslator(context, nodeForErrorReport).TranslateStructMemberVar(reExp), reExp.GetExpType());
    }
    
    TranslationResult<IR0LocResult> IResolvedExpVisitor<TranslationResult<IR0LocResult>>.VisitEnumElemMemberVar(ResolvedExp.EnumElemMemberVar reExp)
    {
        return HandleLocTranslationResult(new CoreResolvedExpIR0LocTranslator(context, nodeForErrorReport).TranslateEnumElemMemberVar(reExp), reExp.GetExpType());
    }

    TranslationResult<IR0LocResult> IResolvedExpVisitor<TranslationResult<IR0LocResult>>.VisitIR0Exp(ResolvedExp.IR0Exp reExp)
    {
        if (bWrapExpAsLoc)
        {
            return HandleLoc(new R.TempLoc(reExp.Exp), reExp.GetExpType());
        }
        else
        {
            context.AddFatalError(notLocationErrorCode, nodeForErrorReport);
            return TranslationResult.Error<IR0LocResult>();
        }
    }

    TranslationResult<IR0LocResult> IResolvedExpVisitor<TranslationResult<IR0LocResult>>.VisitListIndexer(ResolvedExp.ListIndexer reExp)
    {
        return HandleLocTranslationResult(new CoreResolvedExpIR0LocTranslator(context, nodeForErrorReport).TranslateListIndexer(reExp), reExp.GetExpType()); 
    }
}

