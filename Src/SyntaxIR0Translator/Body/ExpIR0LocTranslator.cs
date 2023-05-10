using System;
using System.Diagnostics;
using Citron.Symbol;

using S = Citron.Syntax;
using R = Citron.IR0;

using LocTranslationResult = Citron.Analysis.TranslationResult<Citron.Analysis.IR0LocResult>;
using IExpVisitor = Citron.Syntax.IExpVisitor<Citron.Analysis.TranslationResult<Citron.Analysis.IR0LocResult>>;

using static Citron.Analysis.SyntaxAnalysisErrorCode;
using System.IO;

namespace Citron.Analysis;

partial struct ExpIR0LocTranslator : IExpVisitor
{
    ScopeContext context;
    IType? hintType;
    bool bWrapExpAsLoc;
    bool bDerefIfTypeIsRef;
    S.ISyntaxNode nodeForErrorReport;
    SyntaxAnalysisErrorCode notLocationErrorCode;

    public static LocTranslationResult Translate(S.Exp exp, ScopeContext context, IType? hintType, bool bWrapExpAsLoc, bool bDerefIfTypeIsRef, SyntaxAnalysisErrorCode notLocationErrorCode)
    {
        var translator = new ExpIR0LocTranslator { 
            context = context, 
            hintType = hintType, 
            bWrapExpAsLoc = bWrapExpAsLoc,
            bDerefIfTypeIsRef = bDerefIfTypeIsRef,
            nodeForErrorReport = exp,
            notLocationErrorCode = notLocationErrorCode
        };

        return exp.Accept<ExpIR0LocTranslator, LocTranslationResult>(ref translator);
    }

    LocTranslationResult HandleDefault(S.Exp exp)
    {
        var reExpResult = ExpResolvedExpTranslator.Translate(exp, context, hintType);
        if (!reExpResult.IsValid(out var reExp))
            return TranslationResult.Error<IR0LocResult>();

        return ResolvedExpIR0LocTranslator.Translate(reExp, context, bWrapExpAsLoc, bDerefIfTypeIsRef, exp, A2015_ResolveIdentifier_ExpressionIsNotLocation);
    }

    // fast track
    LocTranslationResult HandleExp(R.Exp exp)
    {
        var expType = exp.GetExpType();

        // bDerefIfTypeIsRef 보다 먼저 bWrapExpAsLoc을 적용시킨다
        if (bWrapExpAsLoc)
        {
            if (bDerefIfTypeIsRef)
            {
                if (expType is LocalRefType localRefType)
                    return TranslationResult.Valid(new IR0LocResult(new R.LocalDerefLoc(new R.TempLoc(exp)), localRefType.InnerType));
                else if (expType is BoxRefType boxRefType)
                    return TranslationResult.Valid(new IR0LocResult(new R.BoxDerefLoc(new R.TempLoc(exp)), boxRefType.InnerType));
            }

            return TranslationResult.Valid(new IR0LocResult(new R.TempLoc(exp), expType));
        }
        else
        {
            context.AddFatalError(notLocationErrorCode, nodeForErrorReport);
            return TranslationResult.Error<IR0LocResult>();
        }
    }

    // fast track
    LocTranslationResult HandleExpTranslationResult(TranslationResult<R.Exp> expResult)
    {
        if (!expResult.IsValid(out var exp))
            return TranslationResult.Error<IR0LocResult>();

        return HandleExp(exp);
    }

    LocTranslationResult IExpVisitor.VisitBinaryOp(S.BinaryOpExp exp)
    {
        var rexp = new CoreExpIR0ExpTranslator(hintType, context).TranslateBinaryOp(exp);
        return HandleExpTranslationResult(rexp);
    }

    LocTranslationResult IExpVisitor.VisitBoolLiteral(S.BoolLiteralExp exp)
    {
        var rexp = new CoreExpIR0ExpTranslator(hintType, context).TranslateBoolLiteral(exp);
        return HandleExp(rexp);
    }

    LocTranslationResult IExpVisitor.VisitBox(S.BoxExp exp)
    {
        throw new NotImplementedException();
    }

    LocTranslationResult IExpVisitor.VisitCall(S.CallExp exp)
    {
        var rexp = new CoreExpIR0ExpTranslator(hintType, context).TranslateCall(exp);
        return HandleExpTranslationResult(rexp);
    }

    LocTranslationResult IExpVisitor.VisitIdentifier(S.IdentifierExp exp)
    {
        return HandleDefault(exp);
    }

    LocTranslationResult IExpVisitor.VisitIndexer(S.IndexerExp exp)
    {
        return HandleDefault(exp);
    }

    LocTranslationResult IExpVisitor.VisitIntLiteral(S.IntLiteralExp exp)
    {
        var rexp = new CoreExpIR0ExpTranslator(hintType, context).TranslateIntLiteral(exp);
        return HandleExp(rexp);
    }

    LocTranslationResult IExpVisitor.VisitLambda(S.LambdaExp exp)
    {
        var rexp = new CoreExpIR0ExpTranslator(hintType, context).TranslateLambda(exp);
        return HandleExp(rexp);
    }

    LocTranslationResult IExpVisitor.VisitList(S.ListExp exp)
    {
        var rexp = new CoreExpIR0ExpTranslator(hintType, context).TranslateList(exp);
        return HandleExpTranslationResult(rexp);
    }

    LocTranslationResult IExpVisitor.VisitMember(S.MemberExp exp)
    {
        return HandleDefault(exp);
    }

    LocTranslationResult IExpVisitor.VisitNew(S.NewExp exp)
    {
        var rexp = new CoreExpIR0ExpTranslator(hintType, context).TranslateNew(exp);
        return HandleExpTranslationResult(rexp);
    }

    LocTranslationResult IExpVisitor.VisitNullLiteral(S.NullLiteralExp exp)
    {
        var rexp = new CoreExpIR0ExpTranslator(hintType, context).TranslateNullLiteral(exp);
        return HandleExpTranslationResult(rexp);
    }

    LocTranslationResult IExpVisitor.VisitString(S.StringExp exp)
    {
        var rexp = new CoreExpIR0ExpTranslator(hintType, context).TranslateString(exp);
        return HandleExpTranslationResult(rexp);
    }

    LocTranslationResult IExpVisitor.VisitUnaryOp(S.UnaryOpExp exp)
    {
        var rexp = new CoreExpIR0ExpTranslator(hintType, context).TranslateUnaryOp(exp);
        return HandleExpTranslationResult(rexp);
    }
}
