
using S = Gum.Syntax;
using R = Gum.IR0;

namespace Gum.IR0Translator
{
    partial class Analyzer
    {
        // GlobalVar 

        struct RootAnalyzer
        {
            RootContext rootContext;
            VarDeclAnalyzer varDeclAnalyzer;
            StmtAndExpAnalyzer analyzer;

            public RootAnalyzer(GlobalContext globalContext, RootContext rootContext, LocalContext localContext)
            {
                this.rootContext = ;
                this.varDeclAnalyzer = new VarDeclAnalyzer();
                this.analyzer = new StmtAndExpAnalyzer(globalContext, rootContext, localContext);
            }

            public void Analyze(S.Script script)
            {
                foreach (var elem in script.Elements)
                {
                    if (elem is S.StmtScriptElement stmtElem)
                        AnalyzeTopLevelStmt(stmtElem.Stmt);
                }
            }

            void AnalyzeTopLevelStmt(S.Stmt stmt)
            {
                if (stmt is S.VarDeclStmt varDeclStmt)
                {
                    var result = varDeclAnalyzer.AnalyzeGlobalVarDecl(varDeclStmt.VarDecl);
                    var stmt = new R.PrivateGlobalVarDeclStmt(result.Elems);
                    rootContext.AddTopLevelStmt(stmt);
                }
                else
                {
                    var stmtResult = analyzer.AnalyzeStmt(stmt);
                    rootContext.AddTopLevelStmt(stmtResult.Stmt);
                }
            }
        }
    }
}
