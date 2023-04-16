using System;
using Citron.Infra;
using Citron.Symbol;

using S = Citron.Syntax;
using R = Citron.IR0;

namespace Citron.Analysis;

// S.Exp -> R.Exp
struct ExpIR0ExpTranslator : S.IExpVisitor<R.Exp>
{
    ScopeContext context;
    IType? hintType;
    S.ISyntaxNode nodeForErrorReport;

    CoreExpIR0ExpTranslator coreExpTranslator;
    
    // ExpResult상위 레이어에 있는게 맞는거 같기도 하다
    public static R.Exp Translate(S.Exp exp, ScopeContext context, IType? hintType)
    {
        var visitor = new ExpIR0ExpTranslator { 
            context = context, hintType = hintType, nodeForErrorReport = exp,
            coreExpTranslator = new CoreExpIR0ExpTranslator(hintType, context)
        };
        return exp.Accept<ExpIR0ExpTranslator, R.Exp>(ref visitor);
    }

    // S.Exp -> IntermediateExp -> ResolvedExp -> R.Exp
    R.Exp HandleDefault(S.Exp exp)
    {   
        var reExp = ExpResolvedExpTranslator.Translate(exp, context, hintType);
        return ResolvedExpIR0ExpTranslator.Translate(reExp);
    }

    R.Exp S.IExpVisitor<R.Exp>.VisitBinaryOp(S.BinaryOpExp exp)
    {
        return coreExpTranslator.TranslateBinaryOp(exp);
    }

    R.Exp S.IExpVisitor<R.Exp>.VisitBoolLiteral(S.BoolLiteralExp exp)
    {
        return coreExpTranslator.TranslateBoolLiteral(exp);
    }

    R.Exp S.IExpVisitor<R.Exp>.VisitBox(S.BoxExp exp)
    {
        throw new NotImplementedException();
    }

    R.Exp S.IExpVisitor<R.Exp>.VisitCall(S.CallExp exp)
    {
        return coreExpTranslator.TranslateCall(exp);
    }
    
    R.Exp S.IExpVisitor<R.Exp>.VisitIdentifier(S.IdentifierExp exp)
    {
        return HandleDefault(exp);
    }

    R.Exp S.IExpVisitor<R.Exp>.VisitIndexer(S.IndexerExp exp)
    {
        return HandleDefault(exp);
    }

    R.Exp S.IExpVisitor<R.Exp>.VisitIntLiteral(S.IntLiteralExp exp)
    {
        return coreExpTranslator.TranslateIntLiteral(exp);
    }

    R.Exp S.IExpVisitor<R.Exp>.VisitLambda(S.LambdaExp exp)
    {
        return coreExpTranslator.TranslateLambda(exp);
    }

    R.Exp S.IExpVisitor<R.Exp>.VisitList(S.ListExp exp)
    {
        return coreExpTranslator.TranslateList(exp);
    }

    R.Exp S.IExpVisitor<R.Exp>.VisitMember(S.MemberExp exp)
    {
        return HandleDefault(exp);
    }

    R.Exp S.IExpVisitor<R.Exp>.VisitNew(S.NewExp exp)
    {
        return coreExpTranslator.TranslateNew(exp);
    }

    R.Exp S.IExpVisitor<R.Exp>.VisitNullLiteral(S.NullLiteralExp exp)
    {
        return coreExpTranslator.TranslateNullLiteral(exp);
    }

    R.Exp S.IExpVisitor<R.Exp>.VisitString(S.StringExp exp)
    {
        return coreExpTranslator.TranslateString(exp);
    }

    R.Exp S.IExpVisitor<R.Exp>.VisitUnaryOp(S.UnaryOpExp exp)
    {
        return coreExpTranslator.TranslateUnaryOp(exp);
    }
}