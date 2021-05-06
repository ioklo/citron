
using S = Gum.Syntax;
using R = Gum.IR0;
using System.Collections.Generic;

using static Gum.IR0Translator.AnalyzeErrorCode;
using Gum.Collections;

namespace Gum.IR0Translator
{
    partial class Analyzer
    {
        // 최상위 script를 분석하는 부분
        struct RootAnalyzer
        {
            GlobalContext globalContext;
            RootContext rootContext;            
            LocalContext localContext;            

            RootAnalyzer(GlobalContext globalContext, RootContext rootContext, LocalContext localContext)
            {
                this.globalContext = globalContext;
                this.rootContext = rootContext;
                this.localContext = localContext;
            }

            public static R.Script Analyze(GlobalContext globalContext, RootContext rootContext, S.Script script)
            {
                var localContext = new LocalContext(rootContext);
                var analyzer = new RootAnalyzer(globalContext, rootContext, localContext);

                var stmtAndExpAnalyzer = new StmtAndExpAnalyzer(globalContext, rootContext, localContext);

                // 첫번째 페이즈, global var를 검사하는 겸 
                foreach (var elem in script.Elements)
                {
                    if (elem is S.StmtScriptElement stmtElem)
                    {
                        if (stmtElem.Stmt is S.VarDeclStmt varDeclStmt)
                        {
                            var rstmt = analyzer.AnalyzeGlobalVarDecl(varDeclStmt.VarDecl);
                            rootContext.AddTopLevelStmt(rstmt);
                        }
                        else
                        {
                            var stmtResult = stmtAndExpAnalyzer.AnalyzeStmt(stmtElem.Stmt);
                            rootContext.AddTopLevelStmt(stmtResult.Stmt);
                        }
                    }
                }

                // 두번째 페이즈, declaration을 훑는다
                var declAnalyzer = new DeclAnalyzer();                
                foreach (var elem in script.Elements)
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

            public R.PrivateGlobalVarDeclStmt AnalyzeGlobalVarDecl(S.VarDecl varDecl)
            {
                var varDeclAnalyzer = new VarDeclElemAnalyzer(globalContext, rootContext, localContext);
                var declType = globalContext.GetTypeValueByTypeExp(varDecl.Type);

                var elems = new List<R.VarDeclElement>();
                foreach (var elem in varDecl.Elems)
                {
                    if (globalContext.DoesInternalGlobalVarNameExist(elem.VarName))
                        globalContext.AddFatalError(A0104_VarDecl_GlobalVariableNameShouldBeUnique, elem);

                    var result = varDeclAnalyzer.AnalyzeVarDeclElement(elem, declType);

                    globalContext.AddInternalGlobalVarInfo(elem.VarName, result.TypeValue);

                    elems.Add(result.Elem);
                }

                return new R.PrivateGlobalVarDeclStmt(elems.ToImmutableArray());
            }
        }
    }
}
