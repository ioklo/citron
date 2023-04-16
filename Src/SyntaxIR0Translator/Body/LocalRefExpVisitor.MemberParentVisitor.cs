using Citron.Symbol;
using Citron.Syntax;

using static Citron.Analysis.SyntaxAnalysisErrorCode;

namespace Citron.Analysis;

partial struct LocalRefExpVisitor
{

    struct MemberParentVisitor : IExpVisitor<MemberParentResult>
    {
        ScopeContext context;

        public MemberParentVisitor(ScopeContext context)
        {
            this.context = context;
        }

        // 제대로 구현해야 할 것
        MemberParentResult IExpVisitor<MemberParentResult>.VisitMember(MemberExp exp)
        {
            var parentVisitor = new MemberParentVisitor();
            var parentResult = exp.Parent.Accept<MemberParentVisitor, MemberParentResult>(ref parentVisitor);

            var typeArgs = BodyMisc.MakeTypeArgs(exp.MemberTypeArgs, context);

            // id랑 결합시키는 과정
            var visitor = new MemberBinder(exp.MemberName, typeArgs, context);
            return parentResult.Accept<MemberBinder, MemberParentResult>(ref visitor);
        }

        MemberParentResult IExpVisitor<MemberParentResult>.VisitIdentifier(IdentifierExp exp)
        {
            // var& x = 'id'.x;
            // id가 local일때, C, S의 정적 멤버변수 일때, struct의 this일때(S& 타입), lambda member var일때 가능
            var typeArgs = BodyMisc.MakeTypeArgs(exp.TypeArgs, context);
            var identifierResult = context.ResolveIdentifier(new Name.Normal(exp.Value), typeArgs);
            if (identifierResult == null)
                context.AddFatalError(A2007_ResolveIdentifier_NotFound, exp);

            var visitor = new IdentifierMemberParentResultVisitor();
            return identifierResult.Accept<IdentifierMemberParentResultVisitor, MemberParentResult>(ref visitor);
        }

        MemberParentResult IExpVisitor<MemberParentResult>.VisitCall(CallExp expSyntax)
        {
            // F(...)가 지역 참조를 리턴하면 가능하다
            // S& F(S& s) { return s; }
            // var s = S(...);
            // var& x = F(ref s).x;

            var exp = CallableAndArgsBinder.Translate(expSyntax, context);
            var expType = exp.GetExpType();

            if (expType is not LocalRefType)
                context.AddFatalError(A3002_LocalRef_ReturnTypeShouldBeLocalRef, expSyntax);

            return new MemberParentResult.Value(exp);
        }
        
        MemberParentResult IExpVisitor<MemberParentResult>.VisitUnaryOp(UnaryOpExp exp)
        {
            // var& x = (i++).x;
            throw new System.NotImplementedException();
        }

        MemberParentResult IExpVisitor<MemberParentResult>.VisitBinaryOp(BinaryOpExp exp)
        {
            // var& x = (2 + 3).x;
            // operation에 따라서(T&를 리턴하는 operation이라면) 그냥 리턴을 할 수도, 에러를 낼 수도 있다
            throw new System.NotImplementedException();
        }

        MemberParentResult IExpVisitor<MemberParentResult>.VisitNullLiteral(NullLiteralExp exp)
        {
            // var& x = null.x;
            // 에러 
            throw new System.NotImplementedException();
        }

        MemberParentResult IExpVisitor<MemberParentResult>.VisitBoolLiteral(BoolLiteralExp exp)
        {
            // var& x = true.x;
            // 에러, temp location을 사용하게 되는데, 그것을 가리킬 수 없다
            throw new System.NotImplementedException();
        }

        MemberParentResult IExpVisitor<MemberParentResult>.VisitIntLiteral(IntLiteralExp exp)
        {
            // var& x = 3.x;
            // 에러, temp location을 사용하게 되는데, 그것을 가리킬 수 없다
            throw new System.NotImplementedException();
        }
        
        MemberParentResult IExpVisitor<MemberParentResult>.VisitString(StringExp exp)
        {
            // var& x = "asdfasdf".x;
            // (에러 를 아니게 만들 수도 있지만 int, bool의 경우와 일관성을 유지하기 위해서 에러 처리.)
            throw new System.NotImplementedException();
        }

        MemberParentResult IExpVisitor<MemberParentResult>.VisitList(ListExp exp)
        {
            // var& x = [1, 2, 3].x
            // 에러, temp location을 사용하게 되는데, 그것을 가리킬 수 없다
            throw new System.NotImplementedException();
        }

        MemberParentResult IExpVisitor<MemberParentResult>.VisitLambda(LambdaExp exp)
        {
            // var& x = (() => { }).x;
            // 에러, temp location을 사용하게 되는데, 그것을 가리킬 수 없다
            throw new System.NotImplementedException();
        }

        MemberParentResult IExpVisitor<MemberParentResult>.VisitBox(BoxExp exp)
        {
            // var& s = box S(); // 에러, 임시 저장소에 들어있던 box 레퍼런스가 날아가게 되면서, s가 invalid해진다.
            throw new System.NotImplementedException();
        }        

        MemberParentResult IExpVisitor<MemberParentResult>.VisitIndexer(IndexerExp exp)
        {
            // var& x = l[i].x;
            // 에러, l[i]는 참조할 수 없다
            throw new System.NotImplementedException();
        }

        MemberParentResult IExpVisitor<MemberParentResult>.VisitNew(NewExp exp)
        {
            // var& x = new C().x;
            // C가 날아가기 때문에, 에러.
            throw new System.NotImplementedException();
        }
    }
}