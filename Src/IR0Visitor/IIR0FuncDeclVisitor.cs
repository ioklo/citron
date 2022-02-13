using R = Citron.IR0;

namespace Citron.IR0Visitor
{
    public interface IIR0FuncDeclVisitor
    {
        void VisitNormalFuncDecl(R.NormalFuncDecl normalFuncDecl);
        void VisitSequenceFuncDecl(R.SequenceFuncDecl seqFuncDecl);
    }
}
