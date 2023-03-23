using S = Citron.Syntax;
using IExpVisitor = Citron.Syntax.IExpVisitor;

namespace Citron.Analysis;

struct RefBuilder : IExpVisitor
{
    void IExpVisitor.VisitBinaryOp(S.BinaryOpExp exp)
    {
        throw new System.NotImplementedException();
    }

    void IExpVisitor.VisitBoolLiteral(S.BoolLiteralExp exp)
    {
        throw new System.NotImplementedException();
    }

    void IExpVisitor.VisitBox(S.BoxExp exp)
    {
        throw new System.NotImplementedException();
    }

    void IExpVisitor.VisitCall(S.CallExp exp)
    {
        throw new System.NotImplementedException();
    }

    void IExpVisitor.VisitIdentifier(S.IdentifierExp exp)
    {
        throw new System.NotImplementedException();
    }

    void IExpVisitor.VisitIndexer(S.IndexerExp exp)
    {
        throw new System.NotImplementedException();
    }

    void IExpVisitor.VisitIntLiteral(S.IntLiteralExp exp)
    {
        throw new System.NotImplementedException();
    }

    void IExpVisitor.VisitLambda(S.LambdaExp exp)
    {
        throw new System.NotImplementedException();
    }

    void IExpVisitor.VisitList(S.ListExp exp)
    {
        throw new System.NotImplementedException();
    }

    void IExpVisitor.VisitMember(S.MemberExp exp)
    {
        throw new System.NotImplementedException();
    }

    void IExpVisitor.VisitNew(S.NewExp exp)
    {
        throw new System.NotImplementedException();
    }

    void IExpVisitor.VisitNullLiteral(S.NullLiteralExp exp)
    {
        throw new System.NotImplementedException();
    }

    void IExpVisitor.VisitRef(S.RefExp exp)
    {
        throw new System.NotImplementedException();
    }

    void IExpVisitor.VisitString(S.StringExp exp)
    {
        throw new System.NotImplementedException();
    }

    void IExpVisitor.VisitUnaryOp(S.UnaryOpExp exp)
    {
        throw new System.NotImplementedException();
    }
}


struct BoxRefExpBuilder : R.IIR0LocVisitor
{
    // public R.Loc? loc; // local(loc)또는 
    R.Exp? result; // boxed exp(boxref, boxmemberref)가 나와야 한다

    public static R.Exp? Build(R.Loc loc)
    {
        var builder = new BoxRefExpBuilder();
        loc.Accept(ref builder);
        return builder.result;
    }

    // &d.c.x => (d.c, "x")
    void R.IIR0LocVisitor.VisitClassMember(R.ClassMemberLoc loc)
    {
        // 가장 첫번째로 만나는 class member

        throw new NotImplementedException();
    }

    // deref도 box용, local용 두개가 생겨야 한다
    void R.IIR0LocVisitor.VisitDerefExp(R.DerefExpLoc loc)
    {
        throw new NotImplementedException();
    }

    // &(*i)
    void R.IIR0LocVisitor.VisitDerefLoc(R.DerefLocLoc loc)
    {
        throw new NotImplementedException();
    }

    // 
    void R.IIR0LocVisitor.VisitEnumElemMember(R.EnumElemMemberLoc loc)
    {
        // TODO: 금지, 에러처리
        throw new NotImplementedException();
    }

    // int x = 0; var f = () => { ... &x ... }
    void R.IIR0LocVisitor.VisitLambdaMemberVar(R.LambdaMemberVarLoc loc)
    {
        // local로 처리한다 (안전하게), TODO: box로 만들고 싶으면 lambda에 표시를 해야한다
        result = null;
    }

    void R.IIR0LocVisitor.VisitListIndexer(R.ListIndexerLoc loc)
    {
        // TODO: ListRef로 감싼다
        throw new NotImplementedException();
    }

    void R.IIR0LocVisitor.VisitLocalVar(R.LocalVarLoc loc)
    {
        // local var의 타입에 따라 달라진다, box int& 였으면, 아니라면 그냥 리턴
        result = new R.BoxRefExp(loc, loc.Type);

        result = null;
    }

    // &nullable.Value
    void R.IIR0LocVisitor.VisitNullableValue(R.NullableValueLoc loc)
    {
        // TODO: 금지, 에러처리
        throw new NotImplementedException();
    }

    // &a.b.c.d
    void R.IIR0LocVisitor.VisitStructMember(R.StructMemberLoc loc)
    {
        if (loc.Instance != null)
        {
            var instanceBoxRefExp = BoxRefExpBuilder.Build(loc.Instance);

            // location이라면 그대로 감싼다
            if (instanceBoxRefExp != null)
            {
                result = new R.BoxRefStructMemberExp(innerBuilder.boxExp, loc.MemberVar);
            }
            else
            {
                result = null;
            }
        }
        else
        {
            result = null;
        }
    }

    void R.IIR0LocVisitor.VisitTemp(R.TempLoc loc)
    {
        throw new NotImplementedException();
    }

    void R.IIR0LocVisitor.VisitThis(R.ThisLoc loc)
    {
        throw new NotImplementedException();
    }
}