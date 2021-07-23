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
            M.ModuleInfo internalModuleInfo;   // 네임스페이스가 추가되면 Analyzer가 따로 생기거나 여기가 polymorphic이 되거나 해야한다

            RootAnalyzer(GlobalContext globalContext, RootContext rootContext, LocalContext localContext, M.ModuleInfo internalModuleInfo)
            {
                this.globalContext = globalContext;
                this.rootContext = rootContext;
                this.localContext = localContext;
                this.internalModuleInfo = internalModuleInfo;
            }

            public static R.Script Analyze(GlobalContext globalContext, RootContext rootContext, M.ModuleInfo internalModuleInfo, S.Script script)
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
                            var globalFuncDecl = analyzer.AnalyzeGlobalFuncDecl(globalFuncDeclElem.FuncDecl);
                            rootContext.AddDecl(globalFuncDecl);

                            break;

                        case S.TypeDeclScriptElement typeDeclElem:
                            var typeDecl = analyzer.AnalyzeTypeDecl(typeDeclElem.TypeDecl);
                            rootContext.AddDecl(typeDecl);
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

            (R.ParamHash ParamHash, ImmutableArray<R.Param> Params) MakeParamHashAndParamInfos(S.FuncDecl funcDecl)
            {
                return Analyzer.MakeParamHashAndParamInfos(globalContext, funcDecl.TypeParams.Length, funcDecl.Parameters);
            }

            static ImmutableArray<R.Path> MakeRTypeArgs(S.GlobalFuncDecl funcDecl)
            {
                var typeArgsBuilder = ImmutableArray.CreateBuilder<R.Path>(funcDecl.TypeParams.Length);

                // NOTICE: global이니까 이전 typeArgs가 없다
                for (int i = 0; i < funcDecl.TypeParams.Length; i++)
                    typeArgsBuilder.Add(new R.Path.TypeVarType(i));
                return typeArgsBuilder.MoveToImmutable();
            }

            R.Path.Nested MakePath(R.Name name, R.ParamHash paramHash, ImmutableArray<R.Path> typeArgs)
            {
                return new R.Path.Nested(rootContext.GetPath(), name, paramHash, typeArgs);
            }
            
            public R.NormalFuncDecl AnalyzeGlobalNormalFuncDecl(S.GlobalFuncDecl funcDecl)
            {
                var retTypeValue = globalContext.GetTypeValueByTypeExp(funcDecl.RetType);

                var rname = new R.Name.Normal(funcDecl.Name);
                var (rparamHash, rparamInfos) = MakeParamHashAndParamInfos(funcDecl);
                var rtypeArgs = MakeRTypeArgs(funcDecl);

                var funcContext = new FuncContext(null, retTypeValue, false, MakePath(rname, rparamHash, rtypeArgs));
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
                
                var decls = funcContext.GetDecls();
                return new R.NormalFuncDecl(decls, rname, false, funcDecl.TypeParams, rparamInfos, bodyResult.Stmt);
            }

            public R.SequenceFuncDecl AnalyzeGlobalSequenceFuncDecl(S.GlobalFuncDecl funcDecl)
            {
                var retTypeValue = globalContext.GetTypeValueByTypeExp(funcDecl.RetType);
                var rname = new R.Name.Normal(funcDecl.Name);
                var (rparamHash, rparamInfos) = MakeParamHashAndParamInfos(funcDecl);
                var rtypeArgs = MakeRTypeArgs(funcDecl);

                var funcContext = new FuncContext(null, retTypeValue, true, MakePath(rname, rparamHash, rtypeArgs));
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

                var decls = funcContext.GetDecls();
                return new R.SequenceFuncDecl(decls, funcDecl.Name, false, retRType, funcDecl.TypeParams, rparamInfos, bodyResult.Stmt);
            }

            R.Decl AnalyzeGlobalFuncDecl(S.GlobalFuncDecl funcDecl)
            {
                if (funcDecl.IsSequence)
                    return AnalyzeGlobalSequenceFuncDecl(funcDecl);
                else
                    return AnalyzeGlobalNormalFuncDecl(funcDecl);
            }

            R.EnumDecl AnalyzeEnumDecl(S.EnumDecl enumDecl)
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

                return new R.EnumDecl(enumDecl.Name, enumDecl.TypeParams, relemsBuilder.MoveToImmutable());
            }
            
            R.Decl AnalyzeTypeDecl(S.TypeDecl typeDecl)
            {
                switch(typeDecl)
                {
                    case S.EnumDecl enumDecl:
                        return AnalyzeEnumDecl(enumDecl);

                    case S.StructDecl structDecl:
                        {

                            // TODO: 현재는 최상위 네임스페이스에서만 찾고 있음
                            var structInfo = GlobalItemQueryService.GetGlobalItem(internalModuleInfo, M.NamespacePath.Root, new ItemPathEntry(structDecl.Name, structDecl.TypeParamCount)) as M.StructInfo;
                            Debug.Assert(structInfo != null);

                            // 이는 TaskStmt 등에서 path를 만들때 사용한다
                            // 고로 path는 TypeVar를 포함해서 만드는 것이 맞다
                            // 여기는 Root이므로 0부터 시작한다
                            var typeArgsBuilder = ImmutableArray.CreateBuilder<TypeValue>(structDecl.TypeParamCount);
                            for (int i = 0; i < structDecl.TypeParamCount; i++)                                
                                typeArgsBuilder.Add(globalContext.MakeTypeVarTypeValue(i));

                            var structTypeValue = globalContext.MakeStructTypeValue(
                                rootContext.MakeRootItemValueOuter(M.NamespacePath.Root),
                                structInfo, 
                                typeArgsBuilder.MoveToImmutable());

                            var structAnalyzer = new StructAnalyzer(globalContext, structDecl, structTypeValue);
                            return structAnalyzer.AnalyzeStructDecl();
                        }

                    default:
                        throw new NotImplementedException();
                }
            }
        }
    }
}
