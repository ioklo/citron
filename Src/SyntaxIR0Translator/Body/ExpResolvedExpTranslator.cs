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
        return HandleExpTranslationResult(new CoreExpIR0ExpTranslator(hintType, context).TranslateUnaryOp(exp));
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

    //// &i를 뭐로 번역할 것인가
    //// &c.x // box ref
    //// &s.x // local ref
    //// &e.x <- 금지, 런타임에 레이아웃이 바뀔 수 있다 (추후 ref-able enum을 쓰면(레이아웃이 겹치지 않는) 되도록 허용할 수 있다)
    //TranslationResult<ResolvedExp> IExpVisitor.VisitRef(S.RefExp exp)
    //{
    //    // &a.b.c.d.e, 일단 innerExp를 memberLoc으로 변경하고, 다시 순회한다
    //    //var innerResult = ExpVisitor.TranslateAsLoc(exp.InnerExp, context, hintType: null, bWrapExpAsLoc: false);
    //    //if (innerResult == null) 
    //    //    throw new NotImplementedException(); // 에러 처리

    //    //var (innerLoc, innerType) = innerResult.Value;

    //    //var refExpBuilder = new BoxRefExpBuilder();
    //    //innerLoc.Accept(ref refExpBuilder);
    //    //refExpBuilder.exp

    //    throw new NotImplementedException();
    //}

    TranslationResult<ResolvedExp> IExpVisitor.VisitBox(S.BoxExp exp)
    {
        throw new NotImplementedException();
    }
}