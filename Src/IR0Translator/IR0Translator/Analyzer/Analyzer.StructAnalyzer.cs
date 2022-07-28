﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using S = Citron.Syntax;
using R = Citron.IR0;
using M = Citron.CompileTime;
using Citron.Infra;
using Citron.Collections;
using Pretune;
using static Citron.IR0Translator.SyntaxAnalysisErrorCode;
using System.Diagnostics;
using Citron.Analysis;

namespace Citron.IR0Translator
{
    partial class Analyzer
    {
        [AutoConstructor]
        partial struct StructAnalyzer
        {
            GlobalContext globalContext;
            S.StructDecl structDecl;
            StructSymbol structSymbol;

            ImmutableArray<R.StructConstructorDecl>.Builder constructorsBuilder;
            ImmutableArray<R.FuncDecl>.Builder memberFuncsBuilder;
            ImmutableArray<R.StructMemberVarDecl>.Builder memberVarsBuilder;

            // Entry
            public static void Analyze(GlobalContext globalContext, ITypeContainer typeContainer, ModuleSymbolId outerId, S.StructDecl structDecl)
            {
                var structSymbol = globalContext.LoadOpenSymbol<StructSymbol>(outerId, structDecl.Name, structDecl.TypeParams, default);
                Debug.Assert(structSymbol != null);

                var constructorsBuilder = ImmutableArray.CreateBuilder<R.StructConstructorDecl>();
                var memberFuncsBuilder = ImmutableArray.CreateBuilder<R.FuncDecl>();
                var memberVarsBuilder = ImmutableArray.CreateBuilder<R.StructMemberVarDecl>();

                var analyzer = new StructAnalyzer(globalContext, structDecl, structSymbol,
                    constructorsBuilder, memberFuncsBuilder, memberVarsBuilder);
                
                R.AccessModifier accessModifier;
                switch (structDecl.AccessModifier)
                {
                    case null: accessModifier = R.AccessModifier.Private; break;
                    case S.AccessModifier.Public: accessModifier = R.AccessModifier.Public; break;
                    case S.AccessModifier.Private: 
                        globalContext.AddFatalError(A2301_RootDecl_CannotSetPrivateAccessExplicitlyBecauseItsDefault, structDecl);
                        throw new UnreachableCodeException();
                    case S.AccessModifier.Protected: accessModifier = R.AccessModifier.Protected; break;
                    default: throw new UnreachableCodeException();
                }

                // MakeBaseTypes
                static ImmutableArray<R.Path> MakeBaseRTypes(ref S.StructDecl structDecl, GlobalContext globalContext)
                {
                    var builder = ImmutableArray.CreateBuilder<R.Path>(structDecl.BaseTypes.Length);
                    foreach (var baseType in structDecl.BaseTypes)
                    {
                        var typeValue = globalContext.GetSymbolByTypeExp(baseType);
                        builder.Add(typeValue.MakeRPath());
                    }
                    return builder.MoveToImmutable();
                }
                var baseTypes = MakeBaseRTypes(ref structDecl, globalContext);

                // TODO: typeParams
                foreach (var elem in structDecl.MemberDecls)
                {
                    analyzer.AnalyzeStructMemberDecl(elem);
                }               

                analyzer.BuildTrivialConstructor();
                
                var rstructDecl = new R.StructDecl(accessModifier, structDecl.Name, structDecl.TypeParams, baseTypes, 
                    constructorsBuilder.ToImmutable(), memberFuncsBuilder.ToImmutable(), memberVarsBuilder.ToImmutable());

                typeContainer.AddType(rstructDecl);
            }

            R.AccessModifier AnalyzeAccessModifier(S.AccessModifier? accessModifier, S.ISyntaxNode nodeForErrorReport)
            {
                switch (accessModifier)
                {
                    case null:
                        return R.AccessModifier.Public;

                    case S.AccessModifier.Private:
                        return R.AccessModifier.Private;

                    case S.AccessModifier.Public:
                        globalContext.AddFatalError(A2401_StructDecl_CannotSetMemberPublicAccessExplicitlyBecauseItsDefault, nodeForErrorReport);
                        break;

                    case S.AccessModifier.Protected:
                        globalContext.AddFatalError(A2402_StructDecl_CannotSetMemberProtectedAccessBecauseItsNotAllowed, nodeForErrorReport);
                        break;
                }

                throw new UnreachableCodeException();
            }

            void AnalyzeMemberVarDecl(S.StructMemberVarDecl varDecl)
            {
                var varTypeValue = globalContext.GetSymbolByTypeExp(varDecl.VarType);
                var rtype = varTypeValue.MakeRPath();

                R.AccessModifier accessModifier = AnalyzeAccessModifier(varDecl.AccessModifier, varDecl);
                memberVarsBuilder.Add(new R.StructMemberVarDecl(accessModifier, rtype, varDecl.VarNames));
            }

            void AnalyzeConstructorDecl(S.StructConstructorDecl constructorDecl)
            {
                R.AccessModifier accessModifier = AnalyzeAccessModifier(constructorDecl.AccessModifier, constructorDecl);

                // name matches struct
                if (constructorDecl.Name != structDecl.Name)
                    globalContext.AddFatalError(A2403_StructDecl_CannotDeclConstructorDifferentWithTypeName, constructorDecl);
                
                var (rparamHash, rparamInfos) = MakeParamHashAndParamInfos(globalContext, 0, constructorDecl.Parameters);

                var constructorPath = new R.Path.Nested(structSymbol.MakeRPath(), R.Name.Constructor.Instance, rparamHash, default);
                var constructorContext = new StructConstructorContext(constructorPath, structSymbol);
                var localContext = new LocalContext();

                // 새로 만든 컨텍스트에 파라미터 순서대로 추가
                foreach (var param in constructorDecl.Parameters)
                {
                    var paramTypeValue = globalContext.GetSymbolByTypeExp(param.Type);
                    localContext.AddLocalVarInfo(param.Kind == S.FuncParamKind.Ref, paramTypeValue, param.Name);
                }

                var analyzer = new StmtAndExpAnalyzer(globalContext, constructorContext, localContext);

                // TODO: Body가 실제로 리턴을 제대로 하는지 확인해야 한다
                var bodyResult = analyzer.AnalyzeStmt(constructorDecl.Body);

                var decls = constructorContext.GetCallableMemberDecls();
                constructorsBuilder.Add(new R.StructConstructorDecl(accessModifier, decls, rparamInfos, bodyResult.Stmt));
            }

            void AnalyzeMemberFuncDecl(S.StructMemberFuncDecl funcDecl)
            {
                if (funcDecl.IsSequence)
                    AnalyzeSequenceFuncDeclElement(funcDecl);
                else
                    AnalyzeNormalFuncDeclElement(funcDecl);
            }

            void AnalyzeSequenceFuncDeclElement(S.StructMemberFuncDecl funcDecl)
            {
                // NOTICE: AnalyzeGlobalSequenceFuncDecl와 비슷한 코드
                var retTypeValue = globalContext.GetSymbolByTypeExp(funcDecl.RetType);
                var rname = new R.Name.Normal(funcDecl.Name);
                var (rparamHash, rparamInfos) = MakeParamHashAndParamInfos(globalContext, funcDecl.TypeParams.Length, funcDecl.Parameters);
                var rtypeArgs = MakeRTypeArgs(0, funcDecl.TypeParams); // NOTICE: global이므로 상위에 type parameter가 없다

                var structPath = structSymbol.MakeRPath();
                var funcPath = new R.Path.Nested(structPath, rname, rparamHash, rtypeArgs);

                var funcContext = new FuncBodyContext(structSymbol, retTypeValue, funcDecl.IsStatic, true, funcPath);
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

                Debug.Assert(retTypeValue != null, "문법상 Sequence 함수의 retValue가 없을수 없습니다");

                var retRType = retTypeValue.MakeRPath();
                var parameters = funcDecl.Parameters.Select(param => param.Name).ToImmutableArray();

                var decls = funcContext.GetCallableMemberDecls();
                var seqFuncDecl = new R.SequenceFuncDecl(decls, new R.Name.Normal(funcDecl.Name), !funcDecl.IsStatic, retRType, funcDecl.TypeParams, rparamInfos, bodyResult.Stmt);

                memberFuncsBuilder.Add(seqFuncDecl);
            }

            void AnalyzeNormalFuncDeclElement(S.StructMemberFuncDecl funcDecl)
            {
                // NOTICE: AnalyzeGlobalNormalFuncDecl와 비슷한 코드                
                var retTypeValue = globalContext.GetSymbolByTypeExp(funcDecl.RetType);

                var rname = new R.Name.Normal(funcDecl.Name);
                var (rparamHash, rparamInfos) = MakeParamHashAndParamInfos(globalContext, funcDecl.TypeParams.Length, funcDecl.Parameters);
                var rtypeArgs = MakeRTypeArgs(structSymbol.GetTotalTypeParamCount(), funcDecl.TypeParams);

                var structPath = structSymbol.MakeRPath();
                var funcPath = new R.Path.Nested(structPath, rname, rparamHash, rtypeArgs);

                var funcContext = new FuncBodyContext(structSymbol, retTypeValue, funcDecl.IsStatic, false, funcPath);
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
                var normalFuncDecl = new R.NormalFuncDecl(decls, rname, !funcDecl.IsStatic, funcDecl.TypeParams, rparamInfos, bodyResult.Stmt);                

                memberFuncsBuilder.Add(normalFuncDecl);
            }

            void AnalyzeStructMemberDecl(S.StructMemberDecl elem)
            {
                switch (elem)
                {
                    case S.StructMemberVarDecl varDecl:
                        AnalyzeMemberVarDecl(varDecl);
                        break;

                    case S.StructConstructorDecl constructorDecl:
                        AnalyzeConstructorDecl(constructorDecl);
                        break;

                    case S.StructMemberFuncDecl funcDecl:
                        AnalyzeMemberFuncDecl(funcDecl);
                        break;

                    default:
                        throw new NotImplementedException();
                }
            }

            void BuildTrivialConstructor()
            {
                var trivialConstructor = structSymbol.GetTrivialConstructor();
                if (trivialConstructor == null) return;

                var structPath = structSymbol.MakeRPath() as R.Path.Nested;
                Debug.Assert(structPath != null);

                var parameterCount = trivialConstructor.GetParameterCount();
                var paramBuilder = ImmutableArray.CreateBuilder<R.Param>(parameterCount);
                var stmtBuilder = ImmutableArray.CreateBuilder<R.Stmt>(parameterCount);

                for(int i = 0; i < parameterCount; i++)
                {
                    var param = trivialConstructor.GetParameter(i);
                    var rname = RItemFactory.MakeName(param.Name);
                    Debug.Assert(rname != null);                    

                    paramBuilder.Add(param.MakeRParam());

                    var structMemberPath = new R.Path.Nested(structPath, rname, R.ParamHash.None, default);
                    stmtBuilder.Add(new R.ExpStmt(new R.AssignExp(new R.StructMemberLoc(R.ThisLoc.Instance, structMemberPath), new R.LoadExp(new R.LocalVarLoc(rname)))));
                }
                var body = new R.BlockStmt(stmtBuilder.MoveToImmutable());

                constructorsBuilder.Add(new R.StructConstructorDecl(R.AccessModifier.Public, default, paramBuilder.MoveToImmutable(), body));
            }
        }
    }
}