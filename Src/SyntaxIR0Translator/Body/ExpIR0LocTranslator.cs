using System;
using System.Diagnostics;
using Citron.Symbol;

using S = Citron.Syntax;
using R = Citron.IR0;

using static Citron.Analysis.SyntaxAnalysisErrorCode;

namespace Citron.Analysis;

struct ExpIR0LocTranslator : S.IExpVisitor<R.Loc>
{
    ScopeContext context;
    IType? hintType;
    bool bWrapExpAsLoc;
    S.ISyntaxNode nodeForErrorReport;

    CoreExpIR0ExpTranslator coreExpTranslator;

    // Translate(operand, context, hintType: null, bWrapExpAsLoc: false);
    public static R.Loc Translate(S.Exp exp, ScopeContext context, IType? hintType, bool bWrapExpAsLoc)
    {
        var translator = new ExpIR0LocTranslator { 
            context = context, 
            hintType = hintType, 
            bWrapExpAsLoc = bWrapExpAsLoc, 
            nodeForErrorReport = exp,
            coreExpTranslator = new CoreExpIR0ExpTranslator(hintType, context) 
        };

        return exp.Accept<ExpIR0LocTranslator, R.Loc>(ref translator);
    }

    R.Loc HandleDefault(S.Exp exp)
    {
        var reExp = ExpResolvedExpTranslator.Translate(exp, context, hintType);
        return ResolvedExpIR0LocTranslator.Translate(reExp, context, bWrapExpAsLoc: bWrapExpAsLoc, exp);
    }

    // fast track
    R.Loc HandleExp(R.Exp exp)
    {
        if (bWrapExpAsLoc)
        {
            return new R.TempLoc(exp);
        }
        else
        {
            context.AddFatalError(A2015_ResolveIdentifier_ExpressionIsNotLocation, nodeForErrorReport);
            throw new UnreachableException();
        }
    }

    R.Loc S.IExpVisitor<R.Loc>.VisitBinaryOp(S.BinaryOpExp exp)
    {
        var rexp = coreExpTranslator.TranslateBinaryOp(exp);
        return HandleExp(rexp);
    }

    R.Loc S.IExpVisitor<R.Loc>.VisitBoolLiteral(S.BoolLiteralExp exp)
    {
        var rexp = coreExpTranslator.TranslateBoolLiteral(exp);
        return HandleExp(rexp);
    }

    R.Loc S.IExpVisitor<R.Loc>.VisitBox(S.BoxExp exp)
    {
        throw new NotImplementedException();
    }

    R.Loc S.IExpVisitor<R.Loc>.VisitCall(S.CallExp exp)
    {
        var rexp = coreExpTranslator.TranslateCall(exp);
        return HandleExp(rexp);
    }

    R.Loc S.IExpVisitor<R.Loc>.VisitIdentifier(S.IdentifierExp exp)
    {
        return HandleDefault(exp);
    }

    R.Loc S.IExpVisitor<R.Loc>.VisitIndexer(S.IndexerExp exp)
    {
        return HandleDefault(exp);
    }

    R.Loc S.IExpVisitor<R.Loc>.VisitIntLiteral(S.IntLiteralExp exp)
    {
        var rexp = coreExpTranslator.TranslateIntLiteral(exp);
        return HandleExp(rexp);
    }

    R.Loc S.IExpVisitor<R.Loc>.VisitLambda(S.LambdaExp exp)
    {
        var rexp = coreExpTranslator.TranslateLambda(exp);
        return HandleExp(rexp);
    }

    R.Loc S.IExpVisitor<R.Loc>.VisitList(S.ListExp exp)
    {
        var rexp = coreExpTranslator.TranslateList(exp);
        return HandleExp(rexp);
    }

    R.Loc S.IExpVisitor<R.Loc>.VisitMember(S.MemberExp exp)
    {
        return HandleDefault(exp);
    }

    R.Loc S.IExpVisitor<R.Loc>.VisitNew(S.NewExp exp)
    {
        var rexp = coreExpTranslator.TranslateNew(exp);
        return HandleExp(rexp);
    }

    R.Loc S.IExpVisitor<R.Loc>.VisitNullLiteral(S.NullLiteralExp exp)
    {
        var rexp = coreExpTranslator.TranslateNullLiteral(exp);
        return HandleExp(rexp);
    }

    R.Loc S.IExpVisitor<R.Loc>.VisitString(S.StringExp exp)
    {
        var rexp = coreExpTranslator.TranslateString(exp);
        return HandleExp(rexp);
    }

    R.Loc S.IExpVisitor<R.Loc>.VisitUnaryOp(S.UnaryOpExp exp)
    {
        var rexp = coreExpTranslator.TranslateUnaryOp(exp);
        return HandleExp(rexp);
    }
}
