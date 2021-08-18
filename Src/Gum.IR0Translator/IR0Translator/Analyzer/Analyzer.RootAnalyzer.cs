using System;
using System.Collections.Generic;
using System.Diagnostics;

using Gum.Infra;
using Gum.Collections;

using static Gum.IR0Translator.AnalyzeErrorCode;

using S = Gum.Syntax;
using R = Gum.IR0;
using M = Gum.CompileTime;

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
            InternalModuleInfo internalModuleInfo;   // 네임스페이스가 추가되면 Analyzer가 따로 생기거나 여기가 polymorphic이 되거나 해야한다

            RootAnalyzer(GlobalContext globalContext, RootContext rootContext, LocalContext localContext, InternalModuleInfo internalModuleInfo)
            {
                this.globalContext = globalContext;
                this.rootContext = rootContext;
                this.localContext = localContext;
                this.internalModuleInfo = internalModuleInfo;
            }

            public static R.Script Analyze(GlobalContext globalContext, RootContext rootContext, InternalModuleInfo internalModuleInfo, S.Script script)
            {
                var localContext = new LocalContext();
                var analyzer = new RootAnalyzer(globalContext, rootContext, localContext, internalModuleInfo);
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

            public R.GlobalVarDeclStmt AnalyzeGlobalVarDecl(S.VarDecl varDecl)
            {
                var varDeclAnalyzer = new VarDeclElemAnalyzer(globalContext, rootContext, localContext);
                var declType = globalContext.GetTypeValueByTypeExp(varDecl.Type);

                var elems = new List<R.VarDeclElement>();
                foreach (var elem in varDecl.Elems)
                {                    
                    if (globalContext.DoesInternalGlobalVarNameExist(elem.VarName))
                        globalContext.AddFatalError(A0104_VarDecl_GlobalVariableNameShouldBeUnique, elem);

                    var result = varDeclAnalyzer.AnalyzeVarDeclElement(bLocal: false, elem, varDecl.IsRef, declType);

                    globalContext.AddInternalGlobalVarInfo(result.Elem is R.VarDeclElement.Ref, result.TypeValue, elem.VarName);

                    elems.Add(result.Elem);
                }

                return new R.GlobalVarDeclStmt(elems.ToImmutableArray());
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
                var retTypeValue = globalContext.GetTypeValueByTypeExp(funcDecl.RetType);

                var rname = new R.Name.Normal(funcDecl.Name);
                var (rparamHash, rparamInfos) = MakeParamHashAndParamInfos(funcDecl);
                var rtypeArgs = MakeRTypeArgs(0, funcDecl.TypeParams); // NOTICE: Global이므로 상위에 type parameter가 없다

                var funcContext = new FuncContext(null, retTypeValue, true, false, MakePath(rname, rparamHash, rtypeArgs));
                var localContext = new LocalContext();
                var analyzer = new StmtAndExpAnalyzer(globalContext, funcContext, localContext);

                // 파라미터 순서대로 추가
                foreach (var param in funcDecl.Parameters)
                {
                    var paramTypeValue = globalContext.GetTypeValueByTypeExp(param.Type);
                    localContext.AddLocalVarInfo(param.Kind == S.FuncParamKind.Ref, paramTypeValue, param.Name);
                }

                // TODO: Body가 실제로 리턴을 제대로 하는지 확인해야 한다
                var bodyResult = analyzer.AnalyzeStmt(funcDecl.Body);
                
                var decls = funcContext.GetCallableMemberDecls();
                var normalFuncDecl = new R.NormalFuncDecl(decls, rname, false, funcDecl.TypeParams, rparamInfos, bodyResult.Stmt);
                var globalFuncDecl = new R.GlobalFuncDecl(normalFuncDecl);
                rootContext.AddGlobalFuncDecl(globalFuncDecl);
            }

            public void AnalyzeGlobalSequenceFuncDecl(S.GlobalFuncDecl funcDecl)
            {
                var retTypeValue = globalContext.GetTypeValueByTypeExp(funcDecl.RetType);
                var rname = new R.Name.Normal(funcDecl.Name);
                var (rparamHash, rparamInfos) = MakeParamHashAndParamInfos(funcDecl);
                var rtypeArgs = MakeRTypeArgs(0, funcDecl.TypeParams); // NOTICE: global이므로 상위에 type parameter가 없다

                var funcContext = new FuncContext(null, retTypeValue, true, true, MakePath(rname, rparamHash, rtypeArgs));
                var localContext = new LocalContext();
                var analyzer = new StmtAndExpAnalyzer(globalContext, funcContext, localContext);

                if (0 < funcDecl.TypeParams.Length)
                    throw new NotImplementedException();

                // 파라미터 순서대로 추가
                foreach (var param in funcDecl.Parameters)
                {
                    var paramTypeValue = globalContext.GetTypeValueByTypeExp(param.Type);
                    localContext.AddLocalVarInfo(param.Kind == S.FuncParamKind.Ref, paramTypeValue, param.Name);
                }

                // TODO: Body가 실제로 리턴을 제대로 하는지 확인해야 한다
                var bodyResult = analyzer.AnalyzeStmt(funcDecl.Body);

                Debug.Assert(retTypeValue != null, "문법상 Sequence 함수의 retValue가 없을수 없습니다");

                var retRType = retTypeValue.GetRPath();
                var parameters = funcDecl.Parameters.Select(param => param.Name).ToImmutableArray();

                var decls = funcContext.GetCallableMemberDecls();
                var seqFuncDecl = new R.SequenceFuncDecl(decls, funcDecl.Name, false, retRType, funcDecl.TypeParams, rparamInfos, bodyResult.Stmt);
                var globalFuncDecl = new R.GlobalFuncDecl(seqFuncDecl);
                rootContext.AddGlobalFuncDecl(globalFuncDecl);
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
                    var rfieldsBuilder = ImmutableArray.CreateBuilder<R.EnumElementField>(elem.Fields.Length);

                    foreach(var param in elem.Fields)
                    {
                        var paramType = globalContext.GetTypeValueByTypeExp(param.Type);
                        var rfield = new R.EnumElementField(paramType.GetRPath(), param.Name);
                        rfieldsBuilder.Add(rfield);
                    }

                    var relem = new R.EnumElement(elem.Name, rfieldsBuilder.MoveToImmutable());
                    relemsBuilder.Add(relem);
                }

                var renumDecl = new R.EnumDecl(enumDecl.Name, enumDecl.TypeParams, relemsBuilder.MoveToImmutable());
                var globalTypeDecl = new R.GlobalTypeDecl(renumDecl);
                rootContext.AddGlobalTypeDecl(globalTypeDecl);
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
                            // TODO: 현재는 최상위 네임스페이스에서만 찾고 있음
                            var structInfo = GlobalItemQueryService.GetGlobalItem(internalModuleInfo, M.NamespacePath.Root, new ItemPathEntry(structDecl.Name, structDecl.TypeParams.Length)) as IModuleStructInfo;
                            Debug.Assert(structInfo != null);

                            // 이는 TaskStmt 등에서 path를 만들때 사용한다
                            // 고로 path는 TypeVar를 포함해서 만드는 것이 맞다
                            // 여기는 Root이므로 0부터 시작한다
                            var typeArgsBuilder = ImmutableArray.CreateBuilder<TypeValue>(structDecl.TypeParams.Length);
                            for (int i = 0; i < structDecl.TypeParams.Length; i++)                                
                                typeArgsBuilder.Add(globalContext.MakeTypeVarTypeValue(i));

                            var structTypeValue = globalContext.MakeStructTypeValue(
                                rootContext.MakeRootItemValueOuter(M.NamespacePath.Root),
                                structInfo, 
                                typeArgsBuilder.MoveToImmutable());

                            StructAnalyzer.Analyze(globalContext, rootContext, structDecl, structTypeValue);
                            break;
                        }

                    case S.ClassDecl classDecl:
                        {
                            // TODO: 현재는 최상위 네임스페이스에서만 찾고 있음
                            var classInfo = GlobalItemQueryService.GetGlobalItem(internalModuleInfo, M.NamespacePath.Root, new ItemPathEntry(classDecl.Name, classDecl.TypeParams.Length)) as IModuleClassInfo;
                            Debug.Assert(classInfo != null);

                            // 이는 TaskStmt 등에서 path를 만들때 사용한다
                            // 고로 path는 TypeVar를 포함해서 만드는 것이 맞다
                            // 여기는 Root이므로 0부터 시작한다
                            var typeArgsBuilder = ImmutableArray.CreateBuilder<TypeValue>(classDecl.TypeParams.Length);
                            for (int i = 0; i < classDecl.TypeParams.Length; i++)
                                typeArgsBuilder.Add(globalContext.MakeTypeVarTypeValue(i));

                            var structTypeValue = globalContext.MakeClassTypeValue(
                                rootContext.MakeRootItemValueOuter(M.NamespacePath.Root),
                                classInfo,
                                typeArgsBuilder.MoveToImmutable());

                            ClassAnalyzer.Analyze(globalContext, rootContext, classDecl, structTypeValue);
                            break;
                        }

                    default:
                        throw new NotImplementedException();
                }
            }
        }
    }
}
