using System;
using Citron.Symbol;

using S = Citron.Syntax;
using R = Citron.IR0;
using IExpVisitor = Citron.Syntax.IExpVisitor<Citron.IR0.Exp>;
using static Citron.Analysis.SyntaxAnalysisErrorCode;
using Citron.Infra;

namespace Citron.Analysis;

// 가장 겉 껍질
// S.Exp, T -> R.Exp (타입이 T&인 값을 만들어주는 exp)
partial struct LocalRefExpVisitor : IExpVisitor
{
    ScopeContext context;
    IType? innerHintType;

    LocalRefExpVisitor(ScopeContext context, IType? innerHintType)
    {
        this.context = context;
        this.innerHintType = innerHintType;
    }

    public static R.Exp TranslateAsLocalRef(S.Exp expSyntax, ScopeContext context, IType? innerHintType) // throws FatalErrorException
    {
        var visitor = new LocalRefExpVisitor(context, innerHintType);
        return expSyntax.Accept<LocalRefExpVisitor, R.Exp>(ref visitor);
    }

    // var& x = 'id'
    R.Exp IExpVisitor.VisitIdentifier(S.IdentifierExp exp) // throws FatalErrorException
    {
        var typeArgs = BodyMisc.MakeTypeArgs(exp.TypeArgs, context);
        var identifierResult = context.ResolveIdentifier(new Name.Normal(exp.Value), typeArgs);
        
        if (identifierResult == null) // 못찾았으면
            context.AddFatalError(A2007_ResolveIdentifier_NotFound, exp);

        var visitor = new IdentifierExpResultVisitor();
        return identifierResult.Accept<IdentifierExpResultVisitor, R.Exp>(ref visitor);
    }

    // var& x = a.b.c.d;
    R.Exp IExpVisitor.VisitMember(S.MemberExp exp)
    {
        // 있는거 갖다 쓰지말고 다 만들어 보자 오래 안 걸린다

        // S.Exp -> RefResult
        var visitor = new MemberParentVisitor();
        var parentResult = exp.Parent.Accept<MemberParentVisitor, MemberParentResult>(ref visitor);

        // exp.Parent는 Namespace, Type등 Exp로 변경 불가능한 것들이 있기 때문에,
        // 변경 가능한것은 최대한 변경한 다음 나머지는 따로 처리해야한다.
        throw new NotImplementedException();
    }

    // var& x = F();
    // 리턴값이 &면 가능
    R.Exp IExpVisitor.VisitCall(S.CallExp expSyntax)
    {
        var exp = CallableAndArgsBinder.Translate(expSyntax, context);

        // 리턴값 체크, local ref만 가능하다
        var expType = exp.GetExpType();

        if (expType is not LocalRefType)        
            context.AddFatalError(A3002_LocalRef_ReturnTypeShouldBeLocalRef, expSyntax);

        return exp;
    }

    // var& b = box S;
    R.Exp IExpVisitor.VisitBox(S.BoxExp exp)
    {
        var innerExp = ExpIR0ExpTranslator.Translate(exp.InnerExp, context, innerHintType);
        return new R.BoxExp(innerExp);
    }
    
    // var& x = l[2];
    R.Exp IExpVisitor.VisitIndexer(S.IndexerExp exp)
    {
        // 비 허용, list전용 ref를 써야 한다
        throw new NotImplementedException();
    }

    // var& x = () => { };
    R.Exp IExpVisitor.VisitLambda(S.LambdaExp exp) => HandleValue(exp);
    
    // var& c = new C();
    R.Exp IExpVisitor.VisitNew(S.NewExp exp)
    {
        // 에러
        throw new NotImplementedException();
    }

    // TODO: 아래 두개는 오퍼레이터에 따라서 다르게 처리해야할 수도 있다 (assign)
    R.Exp IExpVisitor.VisitUnaryOp(S.UnaryOpExp exp)
    {
        throw new NotImplementedException();
    }

    R.Exp IExpVisitor.VisitBinaryOp(S.BinaryOpExp exp)
    {
        throw new NotImplementedException();
    }

    R.Exp IExpVisitor.VisitNullLiteral(S.NullLiteralExp exp) => HandleValue(exp);
    R.Exp IExpVisitor.VisitBoolLiteral(S.BoolLiteralExp exp) => HandleValue(exp);
    R.Exp IExpVisitor.VisitString(S.StringExp exp) => HandleValue(exp);
    R.Exp IExpVisitor.VisitIntLiteral(S.IntLiteralExp exp) => HandleValue(exp);
    R.Exp IExpVisitor.VisitList(S.ListExp exp) => HandleValue(exp);

    R.Exp HandleValue(S.Exp exp)
    {
        context.AddFatalError(A3001_LocalRef_ExpressionIsNotLocation, exp);
        throw new UnreachableException();
    }
}