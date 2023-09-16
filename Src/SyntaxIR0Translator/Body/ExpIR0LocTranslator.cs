﻿using System;
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
    S.ISyntaxNode nodeForErrorReport;
    SyntaxAnalysisErrorCode notLocationErrorCode;

    public static LocTranslationResult Translate(S.Exp exp, ScopeContext context, IType? hintType, bool bWrapExpAsLoc, SyntaxAnalysisErrorCode notLocationErrorCode)
    {
        var translator = new ExpIR0LocTranslator { 
            context = context, 
            hintType = hintType, 
            bWrapExpAsLoc = bWrapExpAsLoc,
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

        return ResolvedExpIR0LocTranslator.Translate(reExp, context, bWrapExpAsLoc, exp, A2015_ResolveIdentifier_ExpressionIsNotLocation);
    }

    // fast track
    LocTranslationResult HandleExp(IR0ExpResult expResult)
    {
        if (bWrapExpAsLoc)
        {
            return TranslationResult.Valid(new IR0LocResult(new R.TempLoc(expResult.Exp, expResult.ExpType), expResult.ExpType));
        }
        else
        {
            context.AddFatalError(notLocationErrorCode, nodeForErrorReport);
            return TranslationResult.Error<IR0LocResult>();
        }
    }

    // fast track
    LocTranslationResult HandleExpTranslationResult(TranslationResult<IR0ExpResult> result)
    {
        if (!result.IsValid(out var expResult))
            return TranslationResult.Error<IR0LocResult>();

        return HandleExp(expResult);
    }

    LocTranslationResult IExpVisitor.VisitBinaryOp(S.BinaryOpExp exp)
    {
        var rexpResult = new CoreExpIR0ExpTranslator(hintType, context).TranslateBinaryOp(exp);
        return HandleExpTranslationResult(rexpResult);
    }

    LocTranslationResult IExpVisitor.VisitBoolLiteral(S.BoolLiteralExp exp)
    {
        var rexp = new CoreExpIR0ExpTranslator(hintType, context).TranslateBoolLiteral(exp);
        return HandleExp(rexp);
    }

    LocTranslationResult IExpVisitor.VisitCall(S.CallExp exp)
    {
        var rexpResult = new CoreExpIR0ExpTranslator(hintType, context).TranslateCall(exp);
        return HandleExpTranslationResult(rexpResult);
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
        var rexpResult = new CoreExpIR0ExpTranslator(hintType, context).TranslateLambda(exp);
        return HandleExpTranslationResult(rexpResult);
    }

    LocTranslationResult IExpVisitor.VisitList(S.ListExp exp)
    {
        var rexpResult = new CoreExpIR0ExpTranslator(hintType, context).TranslateList(exp);
        return HandleExpTranslationResult(rexpResult);
    }

    LocTranslationResult IExpVisitor.VisitMember(S.MemberExp exp)
    {
        return HandleDefault(exp);
    }

    LocTranslationResult IExpVisitor.VisitNew(S.NewExp exp)
    {
        var rexpResult = new CoreExpIR0ExpTranslator(hintType, context).TranslateNew(exp);
        return HandleExpTranslationResult(rexpResult);
    }

    LocTranslationResult IExpVisitor.VisitBox(S.BoxExp exp)
    {
        var rexpResult = new CoreExpIR0ExpTranslator(hintType, context).TranslateBox(exp);
        return HandleExpTranslationResult(rexpResult);
    }

    LocTranslationResult IExpVisitor.VisitIs(S.IsExp exp)
    {
        var rexpResult = new CoreExpIR0ExpTranslator(hintType, context).TranslateIs(exp);
        return HandleExpTranslationResult(rexpResult);
    }

    LocTranslationResult IExpVisitor.VisitAs(S.AsExp exp)
    {
        var rexpResult = new CoreExpIR0ExpTranslator(hintType, context).TranslateAs(exp);
        return HandleExpTranslationResult(rexpResult);
    }

    LocTranslationResult IExpVisitor.VisitNullLiteral(S.NullLiteralExp exp)
    {
        var rexpResult = new CoreExpIR0ExpTranslator(hintType, context).TranslateNullLiteral(exp);
        return HandleExpTranslationResult(rexpResult);
    }

    LocTranslationResult IExpVisitor.VisitString(S.StringExp exp)
    {
        var rexpResult = new CoreExpIR0ExpTranslator(hintType, context).TranslateString_Exp(exp);
        return HandleExpTranslationResult(rexpResult);
    }

    LocTranslationResult IExpVisitor.VisitUnaryOp(S.UnaryOpExp exp)
    {
        // Deref는 loc으로 변경되어야 한다
        if (exp.Kind == S.UnaryOpKind.Deref)
        {
            return HandleDefault(exp);
        }
        else
        {
            var rexpResult = new CoreExpIR0ExpTranslator(hintType, context).TranslateUnaryOpExceptDeref(exp);
            return HandleExpTranslationResult(rexpResult);
        }
    }
}
