﻿using System;
using System.Collections.Generic;
using System.Diagnostics;

using Citron.Infra;
using Citron.Collections;

using static Citron.IR0Translator.SyntaxAnalysisErrorCode;

using S = Citron.Syntax;
using R = Citron.IR0;
using Citron.CompileTime;
using Citron.Analysis;

namespace Citron.IR0Translator
{
    partial class Analyzer
    {
        // 최상위 script를 분석하는 부분
        struct RootAnalyzer
        {
            GlobalContext globalContext;
            RootContext rootContext;
            LocalContext localContext;

            ModuleSymbolId thisId;

            RootAnalyzer(GlobalContext globalContext, RootContext rootContext, LocalContext localContext, ModuleSymbolId moduleId)
            {                
                this.globalContext = globalContext;
                this.rootContext = rootContext;
                this.localContext = localContext;
                this.thisId = moduleId;
            }

            //public static R.Script Analyze(GlobalContext globalContext, RootContext rootContext, Name moduleName, S.Script script)
            //{
            //    var moduleId = new ModuleSymbolId(moduleName, null);
            //    var localContext = new LocalContext();
            //    var analyzer = new RootAnalyzer(globalContext, rootContext, localContext, moduleId);
            //    var stmtAndExpAnalyzer = new StmtAndExpAnalyzer(globalContext, rootContext, localContext);

            //    // 첫번째 페이즈, global var를 검사하는 겸 
            //    foreach (var elem in script.Elements)
            //    {
            //        if (elem is S.StmtScriptElement stmtElem)
            //        {
            //            if (stmtElem.Stmt is S.VarDeclStmt varDeclStmt)
            //            {
            //                var rstmt = analyzer.AnalyzeGlobalVarDecl(varDeclStmt.VarDecl);
            //                rootContext.AddTopLevelStmt(rstmt);
            //            }
            //            else
            //            {
            //                var stmtResult = stmtAndExpAnalyzer.AnalyzeStmt(stmtElem.Stmt);
            //                rootContext.AddTopLevelStmt(stmtResult.Stmt);
            //            }
            //        }
            //    }

            //    // 두번째 페이즈, declaration을 훑는다                
            //    foreach (var elem in script.Elements)
            //    {
            //        switch (elem)
            //        {
            //            case S.GlobalFuncDeclScriptElement globalFuncDeclElem:
            //                analyzer.AnalyzeGlobalFuncDecl(globalFuncDeclElem.FuncDecl);

            //                break;

            //            case S.TypeDeclScriptElement typeDeclElem:
            //                analyzer.AnalyzeTypeDecl(typeDeclElem.TypeDecl);
            //                break;
            //        }
            //    }

            //    return rootContext.MakeScript();
            //}

            public R.GlobalVarDeclStmt AnalyzeGlobalVarDecl(S.VarDecl varDecl)
            {
                //var varDeclAnalyzer = new VarDeclElemAnalyzer(globalContext, rootContext, localContext);
                //var declType = globalContext.GetSymbolByTypeExp(varDecl.Type);

                //var elems = new List<R.VarDeclElement>();
                //foreach (var elem in varDecl.Elems)
                //{                    
                //    if (globalContext.DoesInternalGlobalVarNameExist(elem.VarName))
                //        globalContext.AddFatalError(A0104_VarDecl_GlobalVariableNameShouldBeUnique, elem);

                //    var result = varDeclAnalyzer.AnalyzeVarDeclElement(bLocal: false, elem, varDecl.IsRef, declType);

                //    globalContext.AddInternalGlobalVarInfo(result.Elem is R.VarDeclElement.Ref, result.TypeSymbol, elem.VarName);

                //    elems.Add(result.Elem);
                //}

                //return new R.GlobalVarDeclStmt(elems.ToImmutableArray());
            }

            (R.ParamHash ParamHash, ImmutableArray<R.Param> Params) MakeParamHashAndParamInfos(S.GlobalFuncDecl funcDecl)
            {
                return Analyzer.MakeParamHashAndParamInfos(globalContext, funcDecl.TypeParams.Length, funcDecl.Parameters);
            }
            
            R.Path.Nested MakePath(R.Name name, R.ParamHash paramHash, ImmutableArray<R.Path> typeArgs)
            {
                return new R.Path.Nested(rootContext.GetPath(), name, paramHash, typeArgs);
            }
            
            public void AnalyzeGlobalNormalFuncDecl(S.GlobalFuncDecl funcDecl)
            {
                var retTypeValue = globalContext.GetSymbolByTypeExp(funcDecl.RetType);

                var rname = new R.Name.Normal(funcDecl.Name);
                var (rparamHash, rparamInfos) = MakeParamHashAndParamInfos(funcDecl);
                var rtypeArgs = MakeRTypeArgs(0, funcDecl.TypeParams); // NOTICE: Global이므로 상위에 type parameter가 없다

                var funcContext = new FuncBodyContext(null, retTypeValue, true, false, MakePath(rname, rparamHash, rtypeArgs));
                var localContext = new LocalContext();
                var analyzer = new StmtAndExpAnalyzer(globalContext, funcContext, localContext);

                // 파라미터 순서대로 추가
                foreach (var param in funcDecl.Parameters)
                {
                    var paramTypeValue = globalContext.GetSymbolByTypeExp(param.Type);
                    localContext.AddLocalVarInfo(param.Kind == S.FuncParamKind.Ref, paramTypeValue, param.Name);
                }

                // TODO: Body가 실제로 리턴을 제대로 하는지 확인해야 한다
                var bodyResult = analyzer.AnalyzeStmt(funcDecl.Body);
                
                var decls = funcContext.GetCallableMemberDecls();
                var normalFuncDecl = new R.NormalFuncDecl(decls, rname, false, funcDecl.TypeParams, rparamInfos, bodyResult.Stmt);                
                rootContext.AddGlobalFuncDecl(normalFuncDecl);
            }

            public void AnalyzeGlobalSequenceFuncDecl(S.GlobalFuncDecl funcDecl)
            {
                var retTypeValue = globalContext.GetSymbolByTypeExp(funcDecl.RetType);
                var rname = new R.Name.Normal(funcDecl.Name);
                var (rparamHash, rparamInfos) = MakeParamHashAndParamInfos(funcDecl);
                var rtypeArgs = MakeRTypeArgs(0, funcDecl.TypeParams); // NOTICE: global이므로 상위에 type parameter가 없다

                var funcContext = new FuncBodyContext(null, retTypeValue, true, true, MakePath(rname, rparamHash, rtypeArgs));
                var localContext = new LocalContext();
                var analyzer = new StmtAndExpAnalyzer(globalContext, funcContext, localContext);

                if (0 < funcDecl.TypeParams.Length)
                    throw new NotImplementedException();

                // 파라미터 순서대로 추가
                foreach (var param in funcDecl.Parameters)
                {
                    var paramTypeValue = globalContext.GetSymbolByTypeExp(param.Type);
                    localContext.AddLocalVarInfo(param.Kind == S.FuncParamKind.Ref, paramTypeValue, param.Name);
                }

                // TODO: Body가 실제로 리턴을 제대로 하는지 확인해야 한다
                var bodyResult = analyzer.AnalyzeStmt(funcDecl.Body);

                Debug.Assert(retTypeValue != null, "문법상 Sequence 함수의 retValue가 없을 수 없습니다");

                var retRType = retTypeValue.MakeRPath();
                var parameters = funcDecl.Parameters.Select(param => param.Name).ToImmutableArray();

                var decls = funcContext.GetCallableMemberDecls();
                var seqFuncDecl = new R.SequenceFuncDecl(decls, new R.Name.Normal(funcDecl.Name), false, retRType, funcDecl.TypeParams, rparamInfos, bodyResult.Stmt);                
                rootContext.AddGlobalFuncDecl(seqFuncDecl);
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
                    var rfieldsBuilder = ImmutableArray.CreateBuilder<R.EnumElementField>(elem.MemberVars.Length);

                    foreach(var memberVar in elem.MemberVars)
                    {
                        var paramType = globalContext.GetSymbolByTypeExp(memberVar.Type);
                        var rfield = new R.EnumElementField(paramType.MakeRPath(), memberVar.Name);
                        rfieldsBuilder.Add(rfield);
                    }

                    var relem = new R.EnumElement(elem.Name, rfieldsBuilder.MoveToImmutable());
                    relemsBuilder.Add(relem);
                }

                var renumDecl = new R.EnumDecl(enumDecl.Name, enumDecl.TypeParams, relemsBuilder.MoveToImmutable());
                rootContext.AddGlobalTypeDecl(renumDecl);
            }
            
            void AnalyzeTypeDecl(S.TypeDecl typeDecl)
            {
                switch(typeDecl)
                {
                    case S.EnumDecl enumDecl:
                        AnalyzeEnumDecl(enumDecl);
                        break;

                    case S.StructDecl structDecl:
                        {
                            StructAnalyzer.Analyze(globalContext, rootContext, thisId, structDecl);
                            break;
                        }

                    case S.ClassDecl classDecl:
                        {   
                            ClassAnalyzer.Analyze(globalContext, rootContext, thisId, classDecl);
                            break;
                        }

                    default:
                        throw new NotImplementedException();
                }
            }
        }
    }
}