using Gum.Infra;
using R = Gum.IR0;

namespace Gum.IR0Visitor
{
    public interface IIR0CallableMemberDeclVisitor
    {
        void VisitLambdaDecl(R.LambdaDecl lambdaDecl);
        void VisitCapturedStatementDecl(R.CapturedStatementDecl capturedStatementDecl);
    }
}
