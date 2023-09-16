using System;
using Citron.Infra;
using Citron.Symbol;

using S = Citron.Syntax;
using R = Citron.IR0;

namespace Citron.Analysis;

// S.Exp -> R.Exp
struct ExpIR0ExpTranslator : S.IExpVisitor<TranslationResult<IR0ExpResult>>
{
    ScopeContext context;
    IType? hintType;
    S.ISyntaxNode nodeForErrorReport;
    
    // ExpResult상위 레이어에 있는게 맞는거 같기도 하다
    public static TranslationResult<IR0ExpResult> Translate(S.Exp exp, ScopeContext context, IType? hintType)
    {
        var visitor = new ExpIR0ExpTranslator { 
            context = context, 
            hintType = hintType,
            nodeForErrorReport = exp
        };

        return exp.Accept<ExpIR0ExpTranslator, TranslationResult<IR0ExpResult>>(ref visitor);
    }

    public static TranslationResult<R.StringExp> TranslateString(S.StringExp exp, ScopeContext context, IType? hintType)
    {
        return new CoreExpIR0ExpTranslator(hintType, context).TranslateString_StringExp(exp);
    }

    // S.Exp -> IntermediateExp -> ResolvedExp -> R.Exp
    TranslationResult<IR0ExpResult> HandleDefault(S.Exp exp)
    {   
        var reExpResult = ExpResolvedExpTranslator.Translate(exp, context, hintType);
        if (!reExpResult.IsValid(out var reExp))
            return TranslationResult.Error<IR0ExpResult>();

        return ResolvedExpIR0ExpTranslator.Translate(reExp, context, nodeForErrorReport);
    }

    TranslationResult<IR0ExpResult> Valid(IR0ExpResult result)
    {
        return TranslationResult.Valid(result);
    }

    TranslationResult<IR0ExpResult> S.IExpVisitor<TranslationResult<IR0ExpResult>>.VisitBinaryOp(S.BinaryOpExp exp)
    {
        return new CoreExpIR0ExpTranslator(hintType, context).TranslateBinaryOp(exp);
    }

    TranslationResult<IR0ExpResult> S.IExpVisitor<TranslationResult<IR0ExpResult>>.VisitBoolLiteral(S.BoolLiteralExp exp)
    {
        return Valid(new CoreExpIR0ExpTranslator(hintType, context).TranslateBoolLiteral(exp));
    }

    TranslationResult<IR0ExpResult> S.IExpVisitor<TranslationResult<IR0ExpResult>>.VisitCall(S.CallExp exp)
    {
        return new CoreExpIR0ExpTranslator(hintType, context).TranslateCall(exp);
    }
    
    TranslationResult<IR0ExpResult> S.IExpVisitor<TranslationResult<IR0ExpResult>>.VisitIdentifier(S.IdentifierExp exp)
    {
        return HandleDefault(exp);
    }

    TranslationResult<IR0ExpResult> S.IExpVisitor<TranslationResult<IR0ExpResult>>.VisitIndexer(S.IndexerExp exp)
    {
        return HandleDefault(exp);
    }

    TranslationResult<IR0ExpResult> S.IExpVisitor<TranslationResult<IR0ExpResult>>.VisitIntLiteral(S.IntLiteralExp exp)
    {
        return Valid(new CoreExpIR0ExpTranslator(hintType, context).TranslateIntLiteral(exp));
    }

    TranslationResult<IR0ExpResult> S.IExpVisitor<TranslationResult<IR0ExpResult>>.VisitLambda(S.LambdaExp exp)
    {
        return new CoreExpIR0ExpTranslator(hintType, context).TranslateLambda(exp);
    }

    TranslationResult<IR0ExpResult> S.IExpVisitor<TranslationResult<IR0ExpResult>>.VisitList(S.ListExp exp)
    {
        return new CoreExpIR0ExpTranslator(hintType, context).TranslateList(exp);
    }

    TranslationResult<IR0ExpResult> S.IExpVisitor<TranslationResult<IR0ExpResult>>.VisitMember(S.MemberExp exp)
    {
        return HandleDefault(exp);
    }

    TranslationResult<IR0ExpResult> S.IExpVisitor<TranslationResult<IR0ExpResult>>.VisitNew(S.NewExp exp)
    {
        return new CoreExpIR0ExpTranslator(hintType, context).TranslateNew(exp);
    }

    TranslationResult<IR0ExpResult> S.IExpVisitor<TranslationResult<IR0ExpResult>>.VisitBox(S.BoxExp exp)
    {
        return new CoreExpIR0ExpTranslator(hintType, context).TranslateBox(exp);
    }

    TranslationResult<IR0ExpResult> S.IExpVisitor<TranslationResult<IR0ExpResult>>.VisitIs(S.IsExp exp)
    {
        return new CoreExpIR0ExpTranslator(hintType, context).TranslateIs(exp);
    }

    TranslationResult<IR0ExpResult> S.IExpVisitor<TranslationResult<IR0ExpResult>>.VisitAs(S.AsExp exp)
    {
        return new CoreExpIR0ExpTranslator(hintType, context).TranslateAs(exp);
    }

    TranslationResult<IR0ExpResult> S.IExpVisitor<TranslationResult<IR0ExpResult>>.VisitNullLiteral(S.NullLiteralExp exp)
    {
        return new CoreExpIR0ExpTranslator(hintType, context).TranslateNullLiteral(exp);
    }

    TranslationResult<IR0ExpResult> S.IExpVisitor<TranslationResult<IR0ExpResult>>.VisitString(S.StringExp exp)
    {
        return new CoreExpIR0ExpTranslator(hintType, context).TranslateString_Exp(exp);
    }

    TranslationResult<IR0ExpResult> S.IExpVisitor<TranslationResult<IR0ExpResult>>.VisitUnaryOp(S.UnaryOpExp exp)
    {
        if (exp.Kind == S.UnaryOpKind.Deref)
        {
            return HandleDefault(exp);
        }
        else
        {
            return new CoreExpIR0ExpTranslator(hintType, context).TranslateUnaryOpExceptDeref(exp);
        }
    }
}