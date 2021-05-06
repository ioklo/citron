
using S = Gum.Syntax;
using R = Gum.IR0;
using System.Collections.Generic;

using static Gum.IR0Translator.AnalyzeErrorCode;
using Gum.Collections;
using System;
using System.Diagnostics;

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
                foreach (var elem in script.Elements)
                {
                    switch(elem)
                    {
                        case S.GlobalFuncDeclScriptElement globalFuncDeclElem:
                            analyzer.AnalyzeGlobalFuncDecl(globalFuncDeclElem.FuncDecl);
                            break;

                        case S.TypeDeclScriptElement typeDeclElem:
                            analyzer.AnalyzeTypeDecl(typeDeclElem.TypeDecl);
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


            public void AnalyzeGlobalNormalFuncDecl(S.GlobalFuncDecl funcDecl)
            {
                var retTypeValue = globalContext.GetTypeValueByTypeExp(funcDecl.RetType);
                var funcContext = new FuncContext(retTypeValue, false);
                var localContext = new LocalContext(funcContext);
                var analyzer = new StmtAndExpAnalyzer(globalContext, funcContext, localContext);

                if (0 < funcDecl.TypeParams.Length || funcDecl.ParamInfo.VariadicParamIndex != null)
                    throw new NotImplementedException();

                // 파라미터 순서대로 추가
                foreach (var param in funcDecl.ParamInfo.Parameters)
                {
                    var paramTypeValue = globalContext.GetTypeValueByTypeExp(param.Type);
                    localContext.AddLocalVarInfo(param.Name, paramTypeValue);
                }

                var bodyResult = analyzer.AnalyzeStmt(funcDecl.Body);

                var thisGlobalContext = this.globalContext;

                // TODO: Body가 실제로 리턴을 제대로 하는지 확인해야 한다
                var parameters = MakeParamInfos(funcDecl.ParamInfo.Parameters);

                // 
                var lambdaDecls = funcContext.GetDecls();
                rootContext.AddDecl(new R.NormalFuncDecl(lambdaDecls, funcDecl.Name, false, funcDecl.TypeParams, parameters, bodyResult.Stmt));
            }

            ImmutableArray<R.ParamInfo> MakeParamInfos(ImmutableArray<S.TypeAndName> parameters)
            {
                var builder = ImmutableArray.CreateBuilder<R.ParamInfo>(parameters.Length);

                foreach(var param in parameters)
                {
                    var typeValue = globalContext.GetTypeValueByTypeExp(param.Type);
                    var rtype = typeValue.GetRType();
                    var info = new R.ParamInfo(rtype, param.Name);

                    builder.Add(info);
                }

                return builder.MoveToImmutable();
            }

            public void AnalyzeGlobalSequenceFuncDecl(S.GlobalFuncDecl funcDecl)
            {
                var retTypeValue = globalContext.GetTypeValueByTypeExp(funcDecl.RetType);
                var funcContext = new FuncContext(retTypeValue, true);
                var localContext = new LocalContext(funcContext);
                var analyzer = new StmtAndExpAnalyzer(globalContext, funcContext, localContext);
                
                if (0 < funcDecl.TypeParams.Length || funcDecl.ParamInfo.VariadicParamIndex != null)
                    throw new NotImplementedException();

                // 파라미터 순서대로 추가
                foreach (var param in funcDecl.ParamInfo.Parameters)
                {
                    var paramTypeValue = globalContext.GetTypeValueByTypeExp(param.Type);
                    localContext.AddLocalVarInfo(param.Name, paramTypeValue);
                }

                var bodyResult = analyzer.AnalyzeStmt(funcDecl.Body);

                // TODO: Body가 실제로 리턴을 제대로 하는지 확인해야 한다
                Debug.Assert(retTypeValue != null, "문법상 Sequence 함수의 retValue가 없을수 없습니다");

                var retRType = retTypeValue.GetRType();
                var parameters = funcDecl.ParamInfo.Parameters.Select(param => param.Name).ToImmutableArray();
                var rparamInfos = MakeParamInfos(funcDecl.ParamInfo.Parameters);

                var decls = funcContext.GetDecls();
                rootContext.AddDecl(new R.SequenceFuncDecl(decls, funcDecl.Name, false, retRType, funcDecl.TypeParams, rparamInfos, bodyResult.Stmt));
            }

            void AnalyzeGlobalFuncDecl(S.GlobalFuncDecl funcDecl)
            {
                if (funcDecl.IsSequence)
                    AnalyzeGlobalNormalFuncDecl(funcDecl);
                else
                    AnalyzeGlobalSequenceFuncDecl(funcDecl);
            }

            void AnalyzeTypeDecl(S.TypeDecl typeDecl)
            {
                throw new NotImplementedException();
            }
        }
    }
}
