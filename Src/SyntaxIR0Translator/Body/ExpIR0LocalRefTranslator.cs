using System;
using Citron.Symbol;

using S = Citron.Syntax;
using R = Citron.IR0;
using static Citron.Analysis.SyntaxAnalysisErrorCode;
using Citron.Infra;

namespace Citron.Analysis;

// S.Exp, T -> R.Exp (타입이 local ref T인 값을 만들어주는 exp)
struct ExpIR0LocalRefTranslator
{   
    // hintType은 local ref가 붙지 않은 것을 가정해야하는가 var& 때문이더라도 붙지 않아야 한다
    public static TranslationResult<R.Exp> Translate(S.Exp expSyntax, ScopeContext context, IType? innerHintType) // throws FatalErrorException
    {
        // fast track은 추후에 작성하는 것으로 하고
        var reExpResult = ExpResolvedExpTranslator.Translate(expSyntax, context, innerHintType);
        if (!reExpResult.IsValid(out var reExp))
            return Error();

        return ResolvedExpIR0LocalRefTranslator.Translate(reExp, context, expSyntax);
    }

    static TranslationResult<R.Exp> Error()
    {
        return TranslationResult.Error<R.Exp>();
    }

    //// var& x = 'id'
    //TranslationResult<R.Exp> S.IExpVisitor<TranslationResult<R.Exp>>.VisitIdentifier(S.IdentifierExp exp) // throws FatalErrorException
    //{
    //    var typeArgs = BodyMisc.MakeTypeArgs(exp.TypeArgs, context);
    //    var imExp = context.ResolveIdentifier(new Name.Normal(exp.Value), typeArgs);

    //    if (imExp == null) // 못찾았으면
    //    {
    //        context.AddFatalError(A2007_ResolveIdentifier_NotFound, exp);
    //        return Error();
    //    }

    //    // imExp -> reExp, 확정
    //    var reExp = IntermediateExpResolvedExpTranslator.Translate(imExp, context, exp);

    //    ResolvedExpLocalRefResultTranslator.Translate


    //    var visitor = new IdentifierExpResultVisitor();
    //    return imExp.Accept<IdentifierExpResultVisitor, R.Exp>(ref visitor);
    //}

    //// var& x = a.b.c.d;
    //TranslationResult<R.Exp> S.IExpVisitor<TranslationResult<R.Exp>>.VisitMember(S.MemberExp exp)
    //{
    //    // 있는거 갖다 쓰지말고 다 만들어 보자 오래 안 걸린다

    //    // S.Exp -> RefResult
    //    var visitor = new MemberParentVisitor();
    //    var parentResult = exp.Parent.Accept<MemberParentVisitor, MemberParentResult>(ref visitor);

    //    // exp.Parent는 Namespace, Type등 Exp로 변경 불가능한 것들이 있기 때문에,
    //    // 변경 가능한것은 최대한 변경한 다음 나머지는 따로 처리해야한다.
    //    throw new NotImplementedException();
    //}

    //// var& x = F();
    //// 리턴값이 &면 가능
    //TranslationResult<R.Exp> S.IExpVisitor<TranslationResult<R.Exp>>.VisitCall(S.CallExp expSyntax)
    //{
    //    var exp = CallableAndArgsBinder.Translate(expSyntax, context);

    //    // 리턴값 체크, local ref만 가능하다
    //    var expType = exp.GetExpType();

    //    if (expType is not LocalRefType)
    //    {
    //        context.AddFatalError(A3002_LocalRef_ReturnTypeShouldBeLocalRef, expSyntax);
    //        return Error();
    //    }

    //    return exp;
    //}

    //// var& b = box S;
    //TranslationResult<R.Exp> S.IExpVisitor<TranslationResult<R.Exp>>.VisitBox(S.BoxExp exp)
    //{
    //    var innerExp = ExpIR0ExpTranslator.Translate(exp.InnerExp, context, innerHintType);
    //    return new R.BoxExp(innerExp);
    //}
    
    //// var& x = l[2];
    //TranslationResult<R.Exp> S.IExpVisitor<TranslationResult<R.Exp>>.VisitIndexer(S.IndexerExp exp)
    //{
    //    // 비 허용, list전용 ref를 써야 한다
    //    throw new NotImplementedException();
    //}

    //// var& x = () => { };
    //TranslationResult<R.Exp> S.IExpVisitor<TranslationResult<R.Exp>>.VisitLambda(S.LambdaExp exp) => HandleValue(exp);
    
    //// var& c = new C();
    //TranslationResult<R.Exp> S.IExpVisitor<TranslationResult<R.Exp>>.VisitNew(S.NewExp exp)
    //{
    //    // 에러
    //    throw new NotImplementedException();
    //}

    //// TODO: 아래 두개는 오퍼레이터에 따라서 다르게 처리해야할 수도 있다 (assign)
    //TranslationResult<R.Exp> S.IExpVisitor<TranslationResult<R.Exp>>.VisitUnaryOp(S.UnaryOpExp exp)
    //{
    //    throw new NotImplementedException();
    //}

    //TranslationResult<R.Exp> S.IExpVisitor<TranslationResult<R.Exp>>.VisitBinaryOp(S.BinaryOpExp exp)
    //{
    //    throw new NotImplementedException();
    //}

    //TranslationResult<R.Exp> S.IExpVisitor<TranslationResult<R.Exp>>.VisitNullLiteral(S.NullLiteralExp exp) => HandleValue(exp);
    //TranslationResult<R.Exp> S.IExpVisitor<TranslationResult<R.Exp>>.VisitBoolLiteral(S.BoolLiteralExp exp) => HandleValue(exp);
    //TranslationResult<R.Exp> S.IExpVisitor<TranslationResult<R.Exp>>.VisitString(S.StringExp exp) => HandleValue(exp);
    //TranslationResult<R.Exp> S.IExpVisitor<TranslationResult<R.Exp>>.VisitIntLiteral(S.IntLiteralExp exp) => HandleValue(exp);
    //TranslationResult<R.Exp> S.IExpVisitor<TranslationResult<R.Exp>>.VisitList(S.ListExp exp) => HandleValue(exp);

    //R.Exp HandleValue(S.Exp exp)
    //{
    //    context.AddFatalError(A3001_LocalRef_ExpressionIsNotLocation, exp);
    //    return Error();
    //}
}