﻿
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
                var localContext = new LocalContext();
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
                    switch (elem)
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

            (R.ParamHash ParamHash, R.ParamInfo ParamInfos) MakeParamHashAndParamInfos(S.FuncDecl funcDecl)
            {
                var paramTypesBuilder = ImmutableArray.CreateBuilder<R.Path>(funcDecl.ParamInfo.Parameters.Length);
                var parametersBuilder = ImmutableArray.CreateBuilder<R.TypeAndName>(funcDecl.ParamInfo.Parameters.Length);

                foreach (var param in funcDecl.ParamInfo.Parameters)
                {
                    var typeValue = globalContext.GetTypeValueByTypeExp(param.Type);

                    var type = typeValue.GetRPath();
                    paramTypesBuilder.Add(type);

                    var info = new R.TypeAndName(type, param.Name);
                    parametersBuilder.Add(info);
                }

                var paramHash = new R.ParamHash(funcDecl.TypeParams.Length, paramTypesBuilder.MoveToImmutable());
                var paramInfo = new R.ParamInfo(funcDecl.ParamInfo.VariadicParamIndex, parametersBuilder.MoveToImmutable());

                return (paramHash, paramInfo);
            }

            ImmutableArray<R.Path> MakeRTypeArgs(S.GlobalFuncDecl funcDecl)
            {
                var typeArgsBuilder = ImmutableArray.CreateBuilder<R.Path>(funcDecl.TypeParams.Length);

                // NOTICE: global이니까 이전 typeArgs가 없다
                for (int i = 0; i<funcDecl.TypeParams.Length; i++)
                    typeArgsBuilder.Add(new R.Path.TypeVarType(i));
                return typeArgsBuilder.MoveToImmutable();
            }

            // 
            public void AnalyzeGlobalNormalFuncDecl(S.GlobalFuncDecl funcDecl)
            {
                var retTypeValue = globalContext.GetTypeValueByTypeExp(funcDecl.RetType);

                var rname = new R.Name.Normal(funcDecl.Name);
                var (rparamHash, rparamInfos) = MakeParamHashAndParamInfos(funcDecl);
                var rtypeArgs = MakeRTypeArgs(funcDecl);

                var funcContext = new FuncContext(rootContext, retTypeValue, false, rname, rparamHash, rtypeArgs);
                var localContext = new LocalContext();
                var analyzer = new StmtAndExpAnalyzer(globalContext, funcContext, localContext);

                // 파라미터 순서대로 추가
                foreach (var param in funcDecl.ParamInfo.Parameters)
                {
                    var paramTypeValue = globalContext.GetTypeValueByTypeExp(param.Type);
                    localContext.AddLocalVarInfo(param.Name, paramTypeValue);
                }

                // TODO: Body가 실제로 리턴을 제대로 하는지 확인해야 한다
                var bodyResult = analyzer.AnalyzeStmt(funcDecl.Body);                
                
                var decls = funcContext.GetDecls();
                rootContext.AddDecl(new R.NormalFuncDecl(decls, rname, false, funcDecl.TypeParams, rparamInfos, bodyResult.Stmt));
            }

            public void AnalyzeGlobalSequenceFuncDecl(S.GlobalFuncDecl funcDecl)
            {
                var retTypeValue = globalContext.GetTypeValueByTypeExp(funcDecl.RetType);
                var rname = new R.Name.Normal(funcDecl.Name);
                var (rparamHash, rparamInfos) = MakeParamHashAndParamInfos(funcDecl);
                var rtypeArgs = MakeRTypeArgs(funcDecl);

                var funcContext = new FuncContext(rootContext, retTypeValue, true, rname, rparamHash, rtypeArgs);
                var localContext = new LocalContext();
                var analyzer = new StmtAndExpAnalyzer(globalContext, funcContext, localContext);
                
                if (0 < funcDecl.TypeParams.Length || funcDecl.ParamInfo.VariadicParamIndex != null)
                    throw new NotImplementedException();

                // 파라미터 순서대로 추가
                foreach (var param in funcDecl.ParamInfo.Parameters)
                {
                    var paramTypeValue = globalContext.GetTypeValueByTypeExp(param.Type);
                    localContext.AddLocalVarInfo(param.Name, paramTypeValue);
                }

                // TODO: Body가 실제로 리턴을 제대로 하는지 확인해야 한다
                var bodyResult = analyzer.AnalyzeStmt(funcDecl.Body);
                
                Debug.Assert(retTypeValue != null, "문법상 Sequence 함수의 retValue가 없을수 없습니다");

                var retRType = retTypeValue.GetRPath();
                var parameters = funcDecl.ParamInfo.Parameters.Select(param => param.Name).ToImmutableArray();

                var decls = funcContext.GetDecls();
                rootContext.AddDecl(new R.SequenceFuncDecl(decls, funcDecl.Name, false, retRType, funcDecl.TypeParams, rparamInfos, bodyResult.Stmt));
            }

            void AnalyzeGlobalFuncDecl(S.GlobalFuncDecl funcDecl)
            {
                if (funcDecl.IsSequence)
                    AnalyzeGlobalSequenceFuncDecl(funcDecl);
                else
                    AnalyzeGlobalNormalFuncDecl(funcDecl);
            }

            void AnalyzeEnumDecl(S.EnumDecl enumDecl)
            {
                var relemsBuilder = ImmutableArray.CreateBuilder<R.EnumElement>(enumDecl.Elems.Length);
                foreach(var elem in enumDecl.Elems)
                {
                    var rfieldsBuilder = ImmutableArray.CreateBuilder<R.TypeAndName>(elem.Params.Length);

                    foreach(var param in elem.Params)
                    {
                        var paramType = globalContext.GetTypeValueByTypeExp(param.Type);
                        var rfield = new R.TypeAndName(paramType.GetRPath(), param.Name);
                        rfieldsBuilder.Add(rfield);
                    }

                    var relem = new R.EnumElement(elem.Name, rfieldsBuilder.MoveToImmutable());
                    relemsBuilder.Add(relem);
                }

                rootContext.AddDecl(new R.EnumDecl(enumDecl.Name, enumDecl.TypeParams, relemsBuilder.MoveToImmutable()));
            }

            void AnalyzeTypeDecl(S.TypeDecl typeDecl)
            {
                switch(typeDecl)
                {
                    case S.EnumDecl enumDecl:
                        AnalyzeEnumDecl(enumDecl);
                        break;

                    default:
                        throw new NotImplementedException();
                }
            }
        }
    }
}
