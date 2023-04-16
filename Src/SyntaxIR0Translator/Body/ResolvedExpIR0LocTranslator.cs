using System;
using Citron.Symbol;
using Pretune;

using S = Citron.Syntax;
using R = Citron.IR0;

using static Citron.Analysis.SyntaxAnalysisErrorCode;
using System.Diagnostics;

namespace Citron.Analysis;

record struct LocAndType(R.Loc Loc, IType Type);

partial struct ResolvedExpIR0LocTranslator : IResolvedExpVisitor<R.Loc>
{   
    ScopeContext context;    
    bool bWrapExpAsLoc;
    S.ISyntaxNode nodeForErrorReport;
    CoreResolvedExpIR0LocTranslator coreTranslator;

    public static R.Loc Translate(ResolvedExp reExp, ScopeContext context, bool bWrapExpAsLoc, S.ISyntaxNode nodeForErrorReport)
    {
        var translator = new ResolvedExpIR0LocTranslator { 
            context = context, 
            bWrapExpAsLoc = bWrapExpAsLoc,
            nodeForErrorReport = nodeForErrorReport,
            coreTranslator = new CoreResolvedExpIR0LocTranslator(context, nodeForErrorReport),
        };
        return reExp.Accept<ResolvedExpIR0LocTranslator, R.Loc>(ref translator);
    }    
    
    R.Loc IResolvedExpVisitor<R.Loc>.VisitThis(ResolvedExp.ThisVar reExp)
    {
        return coreTranslator.TranslateThis(reExp);
    }

    R.Loc IResolvedExpVisitor<R.Loc>.VisitLocalVar(ResolvedExp.LocalVar reExp)
    {
        return coreTranslator.TranslateLocalVar(reExp);
    }

    R.Loc IResolvedExpVisitor<R.Loc>.VisitLambdaMemberVar(ResolvedExp.LambdaMemberVar reExp)
    {
        return coreTranslator.TranslateLambdaMemberVar(reExp);
    }
    
    R.Loc IResolvedExpVisitor<R.Loc>.VisitClassMemberVar(ResolvedExp.ClassMemberVar reExp)
    {
        return coreTranslator.TranslateClassMemberVar(reExp);
    }
    
    R.Loc IResolvedExpVisitor<R.Loc>.VisitStructMemberVar(ResolvedExp.StructMemberVar reExp)
    {
        return coreTranslator.TranslateStructMemberVar(reExp);
    }
    
    R.Loc IResolvedExpVisitor<R.Loc>.VisitEnumElemMemberVar(ResolvedExp.EnumElemMemberVar reExp)
    {
        return coreTranslator.TranslateEnumElemMemberVar(reExp);
    }

    R.Loc IResolvedExpVisitor<R.Loc>.VisitIR0Exp(ResolvedExp.IR0Exp reExp)
    {
        if (bWrapExpAsLoc)
            return new R.TempLoc(reExp.Exp);
        else
        {
            context.AddFatalError(A2015_ResolveIdentifier_ExpressionIsNotLocation, nodeForErrorReport);
            throw new UnreachableException();
        }
    }

    R.Loc IResolvedExpVisitor<R.Loc>.VisitListIndexer(ResolvedExp.ListIndexer reExp)
    {
        return coreTranslator.TranslateListIndexer(reExp); 
    }
}

