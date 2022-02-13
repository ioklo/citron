using R = Citron.IR0;

namespace Citron.IR0Visitor
{
    public interface IIR0CallableMemberDeclVisitor
    {
        void VisitLambdaDecl(R.LambdaDecl lambdaDecl);
        void VisitCapturedStatementDecl(R.CapturedStatementDecl capturedStatementDecl);
    }
}
