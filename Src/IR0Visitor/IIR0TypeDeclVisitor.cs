using R = Citron.IR0;

namespace Citron.IR0Visitor
{
    public interface IIR0TypeDeclVisitor
    {
        void VisitStructDecl(R.StructDecl structDecl);
        void VisitClassDecl(R.ClassDecl classDecl);
        void VisitEnumDecl(R.EnumDecl enumDecl);        
    }
}
