using S = Citron.Syntax;
using R = Citron.IR0;
using Citron.Symbol;

namespace Citron.Analysis;

partial struct MemberExpMemberResultTranslator
{
    // S.Exp -> MemberResult
    struct ParentVisitor : S.IExpVisitor<MemberResult>
    {
        ScopeContext context;
        IType? hintType;

        MemberResult S.IExpVisitor<MemberResult>.VisitBinaryOp(S.BinaryOpExp exp)
        {
            var rexp = new ExpIR0ExpTranslator(context, hintType).VisitBinaryOp(exp);
            return new MemberResult.Exp(rexp);
        }

        MemberResult S.IExpVisitor<MemberResult>.VisitBoolLiteral(S.BoolLiteralExp exp)
        {
            var rexp = new ExpIR0ExpTranslator(context).VisitBoolLiteral(exp);
            return new MemberResult.Exp(rexp);
        }

        MemberResult IExpVisitor<MemberResult>.VisitBox(BoxExp exp)
        {
            throw new System.NotImplementedException();
        }

        MemberResult IExpVisitor<MemberResult>.VisitCall(CallExp exp)
        {
            throw new System.NotImplementedException();
        }

        MemberResult IExpVisitor<MemberResult>.VisitIdentifier(IdentifierExp exp)
        {
            throw new System.NotImplementedException();
        }

        MemberResult IExpVisitor<MemberResult>.VisitIndexer(IndexerExp exp)
        {
            throw new System.NotImplementedException();
        }

        MemberResult IExpVisitor<MemberResult>.VisitIntLiteral(IntLiteralExp exp)
        {
            throw new System.NotImplementedException();
        }

        MemberResult IExpVisitor<MemberResult>.VisitLambda(LambdaExp exp)
        {
            throw new System.NotImplementedException();
        }

        MemberResult IExpVisitor<MemberResult>.VisitList(ListExp exp)
        {
            throw new System.NotImplementedException();
        }

        MemberResult IExpVisitor<MemberResult>.VisitMember(MemberExp exp)
        {
            throw new System.NotImplementedException();
        }

        MemberResult IExpVisitor<MemberResult>.VisitNew(NewExp exp)
        {
            throw new System.NotImplementedException();
        }

        MemberResult IExpVisitor<MemberResult>.VisitNullLiteral(NullLiteralExp exp)
        {
            throw new System.NotImplementedException();
        }

        MemberResult IExpVisitor<MemberResult>.VisitString(StringExp exp)
        {
            throw new System.NotImplementedException();
        }

        MemberResult IExpVisitor<MemberResult>.VisitUnaryOp(UnaryOpExp exp)
        {
            throw new System.NotImplementedException();
        }
    }
}
