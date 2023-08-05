using S = Citron.Syntax;
using R = Citron.IR0;
using Citron.Symbol;

namespace Citron.Analysis;

// '&' S.Exp -> R.BoxPtrExp, R.LocalPtrExp
struct RefExpIR0ExpTranslator : S.IExpVisitor<TranslationResult<R.Exp>>
{
    ScopeContext context;

    TranslationResult<R.Exp> S.IExpVisitor<TranslationResult<R.Exp>>.VisitIdentifier(S.IdentifierExp exp)
    {
        
    }

    TranslationResult<R.Exp> S.IExpVisitor<TranslationResult<R.Exp>>.VisitString(S.StringExp exp)
    {
        throw new System.NotImplementedException();
    }

    TranslationResult<R.Exp> S.IExpVisitor<TranslationResult<R.Exp>>.VisitIntLiteral(S.IntLiteralExp exp)
    {
        throw new System.NotImplementedException();
    }

    TranslationResult<R.Exp> S.IExpVisitor<TranslationResult<R.Exp>>.VisitBoolLiteral(S.BoolLiteralExp exp)
    {
        throw new System.NotImplementedException();
    }

    TranslationResult<R.Exp> S.IExpVisitor<TranslationResult<R.Exp>>.VisitNullLiteral(S.NullLiteralExp exp)
    {
        throw new System.NotImplementedException();
    }

    TranslationResult<R.Exp> S.IExpVisitor<TranslationResult<R.Exp>>.VisitBinaryOp(S.BinaryOpExp exp)
    {
        throw new System.NotImplementedException();
    }

    TranslationResult<R.Exp> S.IExpVisitor<TranslationResult<R.Exp>>.VisitUnaryOp(S.UnaryOpExp exp)
    {
        throw new System.NotImplementedException();
    }

    TranslationResult<R.Exp> S.IExpVisitor<TranslationResult<R.Exp>>.VisitCall(S.CallExp exp)
    {
        throw new System.NotImplementedException();
    }

    TranslationResult<R.Exp> S.IExpVisitor<TranslationResult<R.Exp>>.VisitLambda(S.LambdaExp exp)
    {
        throw new System.NotImplementedException();
    }

    TranslationResult<R.Exp> S.IExpVisitor<TranslationResult<R.Exp>>.VisitIndexer(S.IndexerExp exp)
    {
        throw new System.NotImplementedException();
    }

    TranslationResult<R.Exp> S.IExpVisitor<TranslationResult<R.Exp>>.VisitMember(S.MemberExp exp)
    {
        throw new System.NotImplementedException();
    }

    TranslationResult<R.Exp> S.IExpVisitor<TranslationResult<R.Exp>>.VisitList(S.ListExp exp)
    {
        throw new System.NotImplementedException();
    }

    TranslationResult<R.Exp> S.IExpVisitor<TranslationResult<R.Exp>>.VisitNew(S.NewExp exp)
    {
        throw new System.NotImplementedException();
    }

    TranslationResult<R.Exp> S.IExpVisitor<TranslationResult<R.Exp>>.VisitBox(S.BoxExp exp)
    {
        throw new System.NotImplementedException();
    }
}

