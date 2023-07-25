using System;
using Citron.Infra;
using Citron.Symbol;

using S = Citron.Syntax;
using R = Citron.IR0;

namespace Citron.Analysis;

// S.Exp -> R.Exp
struct ExpIR0ExpTranslator : S.IExpVisitor<TranslationResult<R.Exp>>
{
    ScopeContext context;
    IType? hintType;
    S.ISyntaxNode nodeForErrorReport;
    
    // ExpResult상위 레이어에 있는게 맞는거 같기도 하다
    public static TranslationResult<R.Exp> Translate(S.Exp exp, ScopeContext context, IType? hintType)
    {
        var visitor = new ExpIR0ExpTranslator { 
            context = context, 
            hintType = hintType,
            nodeForErrorReport = exp
        };

        return exp.Accept<ExpIR0ExpTranslator, TranslationResult<R.Exp>>(ref visitor);
    }

    public static TranslationResult<R.StringExp> TranslateString(S.StringExp exp, ScopeContext context, IType? hintType)
    {
        return new CoreExpIR0ExpTranslator(hintType, context).TranslateString_StringExp(exp);
    }

    // S.Exp -> IntermediateExp -> ResolvedExp -> R.Exp
    TranslationResult<R.Exp> HandleDefault(S.Exp exp)
    {   
        var reExpResult = ExpResolvedExpTranslator.Translate(exp, context, hintType);
        if (!reExpResult.IsValid(out var reExp))
            return TranslationResult.Error<R.Exp>();

        return ResolvedExpIR0ExpTranslator.Translate(reExp, context, nodeForErrorReport);
    }

    TranslationResult<R.Exp> Valid(R.Exp exp)
    {
        return TranslationResult.Valid(exp);
    }

    TranslationResult<R.Exp> S.IExpVisitor<TranslationResult<R.Exp>>.VisitBinaryOp(S.BinaryOpExp exp)
    {
        return new CoreExpIR0ExpTranslator(hintType, context).TranslateBinaryOp(exp);
    }

    TranslationResult<R.Exp> S.IExpVisitor<TranslationResult<R.Exp>>.VisitBoolLiteral(S.BoolLiteralExp exp)
    {
        return Valid(new CoreExpIR0ExpTranslator(hintType, context).TranslateBoolLiteral(exp));
    }

    TranslationResult<R.Exp> S.IExpVisitor<TranslationResult<R.Exp>>.VisitCall(S.CallExp exp)
    {
        return new CoreExpIR0ExpTranslator(hintType, context).TranslateCall(exp);
    }
    
    TranslationResult<R.Exp> S.IExpVisitor<TranslationResult<R.Exp>>.VisitIdentifier(S.IdentifierExp exp)
    {
        return HandleDefault(exp);
    }

    TranslationResult<R.Exp> S.IExpVisitor<TranslationResult<R.Exp>>.VisitIndexer(S.IndexerExp exp)
    {
        return HandleDefault(exp);
    }

    TranslationResult<R.Exp> S.IExpVisitor<TranslationResult<R.Exp>>.VisitIntLiteral(S.IntLiteralExp exp)
    {
        return Valid(new CoreExpIR0ExpTranslator(hintType, context).TranslateIntLiteral(exp));
    }

    TranslationResult<R.Exp> S.IExpVisitor<TranslationResult<R.Exp>>.VisitLambda(S.LambdaExp exp)
    {
        return new CoreExpIR0ExpTranslator(hintType, context).TranslateLambda(exp);
    }

    TranslationResult<R.Exp> S.IExpVisitor<TranslationResult<R.Exp>>.VisitList(S.ListExp exp)
    {
        return new CoreExpIR0ExpTranslator(hintType, context).TranslateList(exp);
    }

    TranslationResult<R.Exp> S.IExpVisitor<TranslationResult<R.Exp>>.VisitMember(S.MemberExp exp)
    {
        return HandleDefault(exp);
    }

    TranslationResult<R.Exp> S.IExpVisitor<TranslationResult<R.Exp>>.VisitNew(S.NewExp exp)
    {
        return new CoreExpIR0ExpTranslator(hintType, context).TranslateNew(exp);
    }

    TranslationResult<R.Exp> S.IExpVisitor<TranslationResult<R.Exp>>.VisitBox(S.BoxExp exp)
    {
        return new CoreExpIR0ExpTranslator(hintType, context).TranslateBox(exp);
    }

    TranslationResult<R.Exp> S.IExpVisitor<TranslationResult<R.Exp>>.VisitNullLiteral(S.NullLiteralExp exp)
    {
        return new CoreExpIR0ExpTranslator(hintType, context).TranslateNullLiteral(exp);
    }

    TranslationResult<R.Exp> S.IExpVisitor<TranslationResult<R.Exp>>.VisitString(S.StringExp exp)
    {
        return new CoreExpIR0ExpTranslator(hintType, context).TranslateString_Exp(exp);
    }

    TranslationResult<R.Exp> S.IExpVisitor<TranslationResult<R.Exp>>.VisitUnaryOp(S.UnaryOpExp exp)
    {
        if (exp.Kind == S.UnaryOpKind.Deref)
        {
            return HandleDefault(exp);
        }
        else
        {
            return new CoreExpIR0ExpTranslator(hintType, context).TranslateUnaryOp(exp);
        }
    }
}