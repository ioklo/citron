using S = Gum.Syntax;

namespace Gum.IR0Translator
{
    interface ISyntaxScriptVisitor
    {
        void VisitGlobalFuncDecl(S.FuncDecl funcDecl);

        void VisitTopLevelStmt(S.Stmt stmt);

        void VisitTypeDecl(S.TypeDecl typeDecl);
    }
}
