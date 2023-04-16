using System;
using System.Collections.Generic;
using System.Diagnostics;
using Citron.Infra;
using Citron.Collections;
using Citron.Symbol;

using R = Citron.IR0;
using S = Citron.Syntax;
using static Citron.Analysis.SyntaxAnalysisErrorCode;
using IExpVisitor = Citron.Syntax.IExpVisitor<Citron.Analysis.ResolvedExp>;

namespace Citron.Analysis;

// S.Exp -> ResolvedExp, fast track
struct ExpResolvedExpTranslator : IExpVisitor
{
    IType? hintType;
    ScopeContext context;

    CoreExpIR0ExpTranslator coreExpTranslator;

    public static ResolvedExp Translate(S.Exp exp, ScopeContext context, IType? hintType)
    {
        var visitor = new ExpResolvedExpTranslator
        {
            context = context,
            hintType = hintType,
            coreExpTranslator = new CoreExpIR0ExpTranslator(hintType, context)
        };
        return exp.Accept<ExpResolvedExpTranslator, ResolvedExp>(ref visitor);
    }

    public ExpResolvedExpTranslator(ScopeContext context, IType? hintType)
    {
        this.context = context;
        this.hintType = hintType;
    }

    ResolvedExp HandleDefault(S.Exp exp)
    {
        var imExp = ExpIntermediateExpTranslator.Translate(exp, context, hintType);
        return IntermediateExpResolvedExpTranslator.Translate(imExp, context, exp);
    }
    
    ResolvedExp IExpVisitor.VisitIdentifier(S.IdentifierExp exp)
    {
        return HandleDefault(exp);
    }

    // 'null'
    ResolvedExp IExpVisitor.VisitNullLiteral(S.NullLiteralExp exp)
    {
        var rexp = coreExpTranslator.TranslateNullLiteral(exp);
        return new ResolvedExp.IR0Exp(rexp);
    }

    ResolvedExp IExpVisitor.VisitBoolLiteral(S.BoolLiteralExp exp)
    {
        var rexp = coreExpTranslator.TranslateBoolLiteral(exp);
        return new ResolvedExp.IR0Exp(rexp);
    }

    ResolvedExp IExpVisitor.VisitIntLiteral(S.IntLiteralExp exp)
    {
        var rexp = coreExpTranslator.TranslateIntLiteral(exp);
        return new ResolvedExp.IR0Exp(rexp);
    }

    ResolvedExp IExpVisitor.VisitString(S.StringExp exp)
    {
        var rexp = coreExpTranslator.TranslateString(exp);
        return new ResolvedExp.IR0Exp(rexp);
    }

    // int만 지원한다

    ResolvedExp IExpVisitor.VisitUnaryOp(S.UnaryOpExp exp)
    {
        var rexp = coreExpTranslator.TranslateUnaryOp(exp);
        return new ResolvedExp.IR0Exp(rexp);
    }

    ResolvedExp IExpVisitor.VisitBinaryOp(S.BinaryOpExp exp)
    {
        var rexp = coreExpTranslator.TranslateBinaryOp(exp);
        return new ResolvedExp.IR0Exp(rexp);
    }

    ResolvedExp IExpVisitor.VisitCall(S.CallExp exp)
    {
        var rexp = coreExpTranslator.TranslateCall(exp);
        return new ResolvedExp.IR0Exp(rexp);
    }

    ResolvedExp IExpVisitor.VisitLambda(S.LambdaExp exp)
    {
        var rexp = coreExpTranslator.TranslateLambda(exp);
        return new ResolvedExp.IR0Exp(rexp);
    }

    ResolvedExp IExpVisitor.VisitIndexer(S.IndexerExp exp)
    {
        return HandleDefault(exp);
    }

    // exp를 돌려주는 버전
    // parent."x"<>
    ResolvedExp IExpVisitor.VisitMember(S.MemberExp exp)
    {
        return HandleDefault(exp);
    }

    ResolvedExp IExpVisitor.VisitList(S.ListExp exp)
    {
        var rexp = coreExpTranslator.TranslateList(exp);
        return new ResolvedExp.IR0Exp(rexp);
    }

    // 'new C(...)'
    ResolvedExp IExpVisitor.VisitNew(S.NewExp exp)
    {
        var rexp = coreExpTranslator.TranslateNew(exp);
        return new ResolvedExp.IR0Exp(rexp);
    }

    //// &i를 뭐로 번역할 것인가
    //// &c.x // box ref
    //// &s.x // local ref
    //// &e.x <- 금지, 런타임에 레이아웃이 바뀔 수 있다 (추후 ref-able enum을 쓰면(레이아웃이 겹치지 않는) 되도록 허용할 수 있다)
    //ResolvedExp IExpVisitor.VisitRef(S.RefExp exp)
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

    ResolvedExp IExpVisitor.VisitBox(S.BoxExp exp)
    {
        throw new NotImplementedException();
    }
}