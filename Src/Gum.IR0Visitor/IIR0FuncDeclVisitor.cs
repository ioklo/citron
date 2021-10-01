using Gum.Infra;
using R = Gum.IR0;

namespace Gum.IR0Visitor
{
    public interface IIR0FuncDeclVisitor
    {
        void VisitNormalFuncDecl(R.NormalFuncDecl normalFuncDecl);
        void VisitSequenceFuncDecl(R.SequenceFuncDecl seqFuncDecl);
    }
}
