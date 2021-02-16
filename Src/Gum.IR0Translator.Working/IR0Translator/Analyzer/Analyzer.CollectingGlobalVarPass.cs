
using S = Gum.Syntax;

namespace Gum.IR0Translator
{
    partial class Analyzer
    {
        class CollectingGlobalVarPass : ISyntaxScriptVisitor
        {
            Analyzer analyzer;

            public CollectingGlobalVarPass(Analyzer analyzer)
            {
                this.analyzer = analyzer;
            }

            public void VisitTopLevelStmt(S.Stmt stmt)
            {
                analyzer.AnalyzeTopLevelStmt(stmt);
            }

            public void VisitGlobalFuncDecl(S.FuncDecl funcDecl)
            {
                // do nothing
            }

            public void VisitTypeDecl(S.TypeDecl typeDecl)
            {
                // do nothing
            }
        }
        
        void AnalyzeTopLevelStmt(S.Stmt stmt)
        {
            StmtResult stmtResult;

            if (stmt is S.VarDeclStmt varDeclStmt)
                stmtResult = AnalyzeGlobalVarDeclStmt(varDeclStmt);
            else
                stmtResult = AnalyzeCommonStmt(stmt);
            
            context.AddTopLevelStmt(stmtResult.Stmt);
        }
    }
}
