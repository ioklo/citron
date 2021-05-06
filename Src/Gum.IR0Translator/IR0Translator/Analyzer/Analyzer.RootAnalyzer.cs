
using S = Gum.Syntax;
using R = Gum.IR0;

namespace Gum.IR0Translator
{
    partial class Analyzer
    {
        struct RootAnalyzer
        {
            RootContext rootContext;
            VarDeclAnalyzer varDeclAnalyzer;
            StmtAndExpAnalyzer analyzer;

            RootAnalyzer(GlobalContext globalContext, RootContext rootContext, LocalContext localContext)
            {
                this.rootContext = rootContext;
                this.varDeclAnalyzer = new VarDeclAnalyzer();
                this.analyzer = new StmtAndExpAnalyzer(globalContext, rootContext, localContext);
            }

            public R.Script Analyze(GlobalContext globalContext, RootContext rootContext, LocalContext localContext, S.Script script)
            {
                var analyzer = new RootAnalyzer(globalContext, rootContext, localContext);
                var declAnalyzer = new DeclAnalyzer();

                // 첫번째 페이즈, global var를 검사하는 겸 
                foreach (var elem in script.Elements)
                {
                    if (elem is S.StmtScriptElement stmtElem)
                        analyzer.AnalyzeTopLevelStmt(stmtElem.Stmt);
                }

                // 두번째 페이즈, declaration을 훑는다
                foreach(var elem in script.Elements)
                {
                    switch(elem)
                    {
                        case S.GlobalFuncDeclScriptElement globalFuncDeclElem:
                            declAnalyzer.AnalyzeGlobalFuncDecl(globalFuncDeclElem.FuncDecl);
                            break;

                        case S.TypeDeclScriptElement typeDeclElem:
                            declAnalyzer.AnalyzeTypeDecl(typeDeclElem.TypeDecl);
                            break;
                    }
                }

                return rootContext.MakeScript();
            }

            void AnalyzeTopLevelStmt(S.Stmt stmt)
            {
                if (stmt is S.VarDeclStmt varDeclStmt)
                {
                    var result = varDeclAnalyzer.AnalyzeGlobalVarDecl(varDeclStmt.VarDecl);
                    var rstmt = new R.PrivateGlobalVarDeclStmt(result.Elems);
                    rootContext.AddTopLevelStmt(rstmt);
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
