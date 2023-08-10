using System;
using System.Collections.Generic;
using System.Diagnostics;
using Citron.Infra;
using Citron.Collections;
using Citron.Symbol;

using static Citron.Analysis.SyntaxAnalysisErrorCode;

using R = Citron.IR0;
using S = Citron.Syntax;
using IExpVisitor = Citron.Syntax.IExpVisitor<Citron.Analysis.TranslationResult<Citron.Analysis.ResolvedExp>>;

namespace Citron.Analysis;

// S.Exp -> ResolvedExp, fast track
struct ExpResolvedExpTranslator : IExpVisitor
{
    IType? hintType;
    ScopeContext context;

    public static TranslationResult<ResolvedExp> Translate(S.Exp exp, ScopeContext context, IType? hintType)
    {
        var visitor = new ExpResolvedExpTranslator
        {
            context = context,
            hintType = hintType
        };
        return exp.Accept<ExpResolvedExpTranslator, TranslationResult<ResolvedExp>>(ref visitor);
    }

    public ExpResolvedExpTranslator(ScopeContext context, IType? hintType)
    {
        this.context = context;
        this.hintType = hintType;
    }

    TranslationResult<ResolvedExp> HandleDefault(S.Exp exp)
    {
        var imExpResult = ExpIntermediateExpTranslator.Translate(exp, context, hintType);
        if (!imExpResult.IsValid(out var imExp))
            return Error();

        return IntermediateExpResolvedExpTranslator.Translate(imExp, context, exp);
    }

    TranslationResult<ResolvedExp> Error()
    {
        return TranslationResult.Error<ResolvedExp>();
    }

    TranslationResult<ResolvedExp> HandleExp(R.Exp exp)
    {
        return TranslationResult.Valid<ResolvedExp>(new ResolvedExp.IR0Exp(exp));
    }

    TranslationResult<ResolvedExp> HandleExpTranslationResult(TranslationResult<R.Exp> expResult)
    {
        if (!expResult.IsValid(out var exp))
            return Error();

        return HandleExp(exp);
    }
    
    TranslationResult<ResolvedExp> IExpVisitor.VisitIdentifier(S.IdentifierExp exp)
    {
        return HandleDefault(exp);
    }

    TranslationResult<ResolvedExp> Valid(ResolvedExp reExp)
    {
        return TranslationResult.Valid(reExp);
    }

    // 'null'
    TranslationResult<ResolvedExp> IExpVisitor.VisitNullLiteral(S.NullLiteralExp exp)
    {   
        return HandleExpTranslationResult(new CoreExpIR0ExpTranslator(hintType, context).TranslateNullLiteral(exp));
    }

    TranslationResult<ResolvedExp> IExpVisitor.VisitBoolLiteral(S.BoolLiteralExp exp)
    {   
        return HandleExp(new CoreExpIR0ExpTranslator(hintType, context).TranslateBoolLiteral(exp));
    }

    TranslationResult<ResolvedExp> IExpVisitor.VisitIntLiteral(S.IntLiteralExp exp)
    {
        return HandleExp(new CoreExpIR0ExpTranslator(hintType, context).TranslateIntLiteral(exp));
    }

    TranslationResult<ResolvedExp> IExpVisitor.VisitString(S.StringExp exp)
    {
        return HandleExpTranslationResult(new CoreExpIR0ExpTranslator(hintType, context).TranslateString_Exp(exp));
    }

    // int만 지원한다
    TranslationResult<ResolvedExp> IExpVisitor.VisitUnaryOp(S.UnaryOpExp exp)
    {
        if (exp.Kind == S.UnaryOpKind.Deref)
        {
            return HandleDefault(exp);
        }
        else
        {
            return HandleExpTranslationResult(new CoreExpIR0ExpTranslator(hintType, context).TranslateUnaryOpExceptDeref(exp));
        }
    }

    TranslationResult<ResolvedExp> IExpVisitor.VisitBinaryOp(S.BinaryOpExp exp)
    {
        return HandleExpTranslationResult(new CoreExpIR0ExpTranslator(hintType, context).TranslateBinaryOp(exp));
    }

    TranslationResult<ResolvedExp> IExpVisitor.VisitCall(S.CallExp exp)
    {
        return HandleExpTranslationResult(new CoreExpIR0ExpTranslator(hintType, context).TranslateCall(exp));
    }

    TranslationResult<ResolvedExp> IExpVisitor.VisitLambda(S.LambdaExp exp)
    {
        return HandleExpTranslationResult(new CoreExpIR0ExpTranslator(hintType, context).TranslateLambda(exp));
    }

    TranslationResult<ResolvedExp> IExpVisitor.VisitIndexer(S.IndexerExp exp)
    {
        return HandleDefault(exp);
    }

    // exp를 돌려주는 버전
    // parent."x"<>
    TranslationResult<ResolvedExp> IExpVisitor.VisitMember(S.MemberExp exp)
    {
        return HandleDefault(exp);
    }

    TranslationResult<ResolvedExp> IExpVisitor.VisitList(S.ListExp exp)
    {
        return HandleExpTranslationResult(new CoreExpIR0ExpTranslator(hintType, context).TranslateList(exp));
    }

    // 'new C(...)'
    TranslationResult<ResolvedExp> IExpVisitor.VisitNew(S.NewExp exp)
    {
        return HandleExpTranslationResult(new CoreExpIR0ExpTranslator(hintType, context).TranslateNew(exp));
    }
    
    TranslationResult<ResolvedExp> IExpVisitor.VisitBox(S.BoxExp exp)
    {
        return HandleExpTranslationResult(new CoreExpIR0ExpTranslator(hintType, context).TranslateBox(exp));
    }
}