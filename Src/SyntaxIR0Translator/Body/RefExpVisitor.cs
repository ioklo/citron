using System;
using Citron.Symbol;

using S = Citron.Syntax;
using R = Citron.IR0;
using IExpVisitor = Citron.Syntax.IExpVisitor<Citron.Analysis.ExpResult>;

namespace Citron.Analysis;

struct RefExpVisitor : IExpVisitor
{
    public static R.LocalRefExp TranslateAsLocalRef(S.Exp expSyntax, ScopeContext context, IType hintInnerType) // throws FatalErrorException
    {
        var visitor = new RefExpVisitor();
        // expSyntax.Accept(ref visitor);
    }

    ExpResult IExpVisitor.VisitBinaryOp(S.BinaryOpExp exp)
    {
        throw new System.NotImplementedException();
    }

    ExpResult IExpVisitor.VisitBoolLiteral(S.BoolLiteralExp exp)
    {
        throw new System.NotImplementedException();
    }

    ExpResult IExpVisitor.VisitBox(S.BoxExp exp)
    {
        throw new System.NotImplementedException();
    }

    ExpResult IExpVisitor.VisitCall(S.CallExp exp)
    {
        throw new System.NotImplementedException();
    }

    ExpResult IExpVisitor.VisitIdentifier(S.IdentifierExp exp)
    {
        throw new System.NotImplementedException();
    }

    ExpResult IExpVisitor.VisitIndexer(S.IndexerExp exp)
    {
        throw new System.NotImplementedException();
    }

    ExpResult IExpVisitor.VisitIntLiteral(S.IntLiteralExp exp)
    {
        throw new System.NotImplementedException();
    }

    ExpResult IExpVisitor.VisitLambda(S.LambdaExp exp)
    {
        throw new System.NotImplementedException();
    }

    ExpResult IExpVisitor.VisitList(S.ListExp exp)
    {
        throw new System.NotImplementedException();
    }

    ExpResult IExpVisitor.VisitMember(S.MemberExp exp)
    {
        throw new System.NotImplementedException();
    }

    ExpResult IExpVisitor.VisitNew(S.NewExp exp)
    {
        throw new System.NotImplementedException();
    }

    ExpResult IExpVisitor.VisitNullLiteral(S.NullLiteralExp exp)
    {
        throw new System.NotImplementedException();
    }

    ExpResult IExpVisitor.VisitString(S.StringExp exp)
    {
        throw new System.NotImplementedException();
    }

    ExpResult IExpVisitor.VisitUnaryOp(S.UnaryOpExp exp)
    {
        throw new System.NotImplementedException();
    }
}