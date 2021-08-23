﻿using System;
using System.Linq;

using S = Gum.Syntax;
using R = Gum.IR0;
using M = Gum.CompileTime;
using Gum.Infra;
using Gum.Collections;
using Pretune;
using static Gum.IR0Translator.AnalyzeErrorCode;
using System.Diagnostics;

namespace Gum.IR0Translator
{
    partial class Analyzer
    {
        [AutoConstructor]
        partial struct ClassAnalyzer
        {
            GlobalContext globalContext;
            ITypeContainer typeContainer;
            S.ClassDecl classDecl;
            ClassTypeValue classTypeValue;

            ImmutableArray<R.ClassConstructorDecl>.Builder constructorsBuilder;
            ImmutableArray<R.FuncDecl>.Builder memberFuncsBuilder;
            ImmutableArray<R.ClassMemberVarDecl>.Builder memberVarsBuilder;

            // Entry
            public static void Analyze(GlobalContext globalContext, ITypeContainer typeContainer, S.ClassDecl classDecl, ClassTypeValue classTypeValue)
            {
                var constructorsBuilder = ImmutableArray.CreateBuilder<R.ClassConstructorDecl>();
                var memberFuncsBuilder = ImmutableArray.CreateBuilder<R.FuncDecl>();
                var memberVarsBuilder = ImmutableArray.CreateBuilder<R.ClassMemberVarDecl>();
                
                var classAnalyzer = new ClassAnalyzer(globalContext, typeContainer, classDecl, classTypeValue,
                    constructorsBuilder, memberFuncsBuilder, memberVarsBuilder);
                
                R.AccessModifier accessModifier;
                switch (classDecl.AccessModifier)
                {
                    case null: accessModifier = R.AccessModifier.Private; break;
                    case S.AccessModifier.Public: accessModifier = R.AccessModifier.Public; break;
                    case S.AccessModifier.Private:
                        globalContext.AddFatalError(A2301_RootDecl_CannotSetPrivateAccessExplicitlyBecauseItsDefault, classDecl);
                        throw new UnreachableCodeException();
                    case S.AccessModifier.Protected: accessModifier = R.AccessModifier.Protected; break;
                    default: throw new UnreachableCodeException();
                }

                // MakeBaseTypes
                static R.Path.Nested? MakeBaseType(ref S.ClassDecl classDecl, GlobalContext globalContext)
                {   
                    // IntenralModuleInfoBuilder에서 처리하기 때문에 여기서는 Base가 두개 이상 나올일이 없다
                    foreach (var baseType in classDecl.BaseTypes)
                    {
                        var typeValue = globalContext.GetTypeValueByTypeExp(baseType);
                        if (typeValue is ClassTypeValue classTypeValue)
                            return classTypeValue.GetRPath_Nested();
                    }

                    return null;
                }
                var baseType = MakeBaseType(ref classDecl, globalContext);

                // TODO: typeParams
                
                foreach (var elem in classDecl.MemberDecls)
                {
                    classAnalyzer.AnalyzeMemberDecl(elem);
                }

                classAnalyzer.BuildAutomaticConstructor();
                
                var rclassDecl = new R.ClassDecl(accessModifier, classDecl.Name, classDecl.TypeParams, baseType, default,
                    constructorsBuilder.ToImmutable(), memberFuncsBuilder.ToImmutable(), memberVarsBuilder.ToImmutable());

                typeContainer.AddType(rclassDecl);
            }

            R.AccessModifier AnalyzeAccessModifier(S.AccessModifier? accessModifier, S.ISyntaxNode nodeForErrorReport)
            {
                switch (accessModifier)
                {
                    case null:
                        return R.AccessModifier.Private;

                    case S.AccessModifier.Public:
                        return R.AccessModifier.Public;

                    case S.AccessModifier.Private:
                        globalContext.AddFatalError(A2501_ClassDecl_CannotSetMemberPrivateAccessExplicitlyBecauseItsDefault, nodeForErrorReport);
                        break;

                    case S.AccessModifier.Protected:
                        return R.AccessModifier.Protected;
                }

                throw new UnreachableCodeException();
            }

            void AnalyzeMemberVarDecl(S.ClassMemberVarDecl varDecl)
            {
                var varTypeValue = globalContext.GetTypeValueByTypeExp(varDecl.VarType);
                var rtype = varTypeValue.GetRPath();

                R.AccessModifier accessModifier = AnalyzeAccessModifier(varDecl.AccessModifier, varDecl);
                memberVarsBuilder.Add(new R.ClassMemberVarDecl(accessModifier, rtype, varDecl.VarNames));
            }

            R.ConstructorBaseCallInfo? HandleConstructorDeclBaseArgs(ICallableContext callableContext, LocalContext localContext, ImmutableArray<S.Argument>? baseArgs, S.ISyntaxNode nodeForErrorReport)
            {
                if (baseArgs == null)
                {
                    var baseTypeValue = classTypeValue.GetBaseType();
                    if (baseTypeValue == null)
                        return null;
                    
                    // BaseType이 있다면 기본을 불러야 한다. 
                    // 기본이 있는가
                    var constructor = baseTypeValue.GetDefaultConstructor();
                    if (constructor == null)
                    {
                        globalContext.AddFatalError(A2503_ClassDecl_CannotFindBaseClassConstructor, nodeForErrorReport);
                        throw new UnreachableCodeException();
                    }

                    if (!constructor.CheckAccess(classTypeValue))
                        globalContext.AddFatalError(A2504_ClassDecl_CannotAccessBaseClassConstructor, nodeForErrorReport);

                    // no argument
                    return new R.ConstructorBaseCallInfo(R.ParamHash.None, default);
                }
                else
                {
                    // 인자가 있는 상황
                    var baseTypeValue = classTypeValue.GetBaseType();

                    // 인자가 있는데 부모 클래스가 없으면,
                    if (baseTypeValue == null)
                    {
                        globalContext.AddFatalError(A2505_ClassDecl_TryCallBaseConstructorWithoutBaseClass, nodeForErrorReport);
                    }
                    else
                    {
                        var result = baseTypeValue.GetMember(M.SpecialNames.Constructor, typeParamCount: 0); // NOTICE: constructor는 타입 파라미터가 없다
                        switch (result)
                        {
                            case ItemQueryResult.Constructors constructorResult:
                                var matchedConstructor = FuncMatcher.Match(globalContext, callableContext, localContext, baseTypeValue.MakeTypeEnv(), constructorResult.ConstructorInfos, baseArgs.Value, default, nodeForErrorReport);

                                var constructorValue = globalContext.MakeConstructorValue(constructorResult.Outer, matchedConstructor.CallableInfo);

                                if (!constructorValue.CheckAccess(classTypeValue))
                                    globalContext.AddFatalError(A2504_ClassDecl_CannotAccessBaseClassConstructor, nodeForErrorReport);

                                return new R.ConstructorBaseCallInfo(constructorValue.GetRPath_Nested().ParamHash, matchedConstructor.Args);

                            case ItemQueryResult.NotFound:
                                globalContext.AddFatalError(A2503_ClassDecl_CannotFindBaseClassConstructor, nodeForErrorReport);
                                break;

                            case ItemQueryResult.Error errorResult:
                                HandleItemQueryResultError(globalContext, errorResult, nodeForErrorReport);
                                break;
                        }
                    }
                }

                throw new UnreachableCodeException();
            }

            void AnalyzeConstructorDecl(S.ClassConstructorDecl constructorDecl)
            {
                R.AccessModifier accessModifier = AnalyzeAccessModifier(constructorDecl.AccessModifier, constructorDecl);

                // name matches struct
                if (constructorDecl.Name != classDecl.Name)
                    globalContext.AddFatalError(A2502_ClassDecl_CannotDeclConstructorDifferentWithTypeName, constructorDecl);

                var (rparamHash, rparamInfos) = MakeParamHashAndParamInfos(globalContext, 0, constructorDecl.Parameters);

                var constructorPath = new R.Path.Nested(classTypeValue.GetRPath_Nested(), R.Name.Constructor.Instance, rparamHash, default);
                var constructorContext = new ClassConstructorContext(constructorPath, classTypeValue);
                var localContext = new LocalContext();

                // 새로 만든 컨텍스트에 파라미터 순서대로 추가
                foreach (var param in constructorDecl.Parameters)
                {
                    var paramTypeValue = globalContext.GetTypeValueByTypeExp(param.Type);
                    localContext.AddLocalVarInfo(param.Kind == S.FuncParamKind.Ref, paramTypeValue, param.Name);
                }

                R.ConstructorBaseCallInfo? baseCallInfo = HandleConstructorDeclBaseArgs(constructorContext, localContext, constructorDecl.BaseArgs, constructorDecl);

                var analyzer = new StmtAndExpAnalyzer(globalContext, constructorContext, localContext);

                // TODO: Body가 실제로 리턴을 제대로 하는지 확인해야 한다
                var bodyResult = analyzer.AnalyzeStmt(constructorDecl.Body);

                var decls = constructorContext.GetCallableMemberDecls();

                constructorsBuilder.Add(new R.ClassConstructorDecl(accessModifier, decls, rparamInfos, baseCallInfo, bodyResult.Stmt));
            }

            void AnalyzeMemberFuncDecl(S.ClassMemberFuncDecl funcDecl)
            {
                if (funcDecl.IsSequence)
                    AnalyzeSequenceFuncDeclElement(funcDecl);
                else
                    AnalyzeNormalFuncDeclElement(funcDecl);
            }

            void AnalyzeSequenceFuncDeclElement(S.ClassMemberFuncDecl funcDecl)
            {
                // NOTICE: AnalyzeGlobalSequenceFuncDecl와 비슷한 코드
                var retTypeValue = globalContext.GetTypeValueByTypeExp(funcDecl.RetType);
                var rname = new R.Name.Normal(funcDecl.Name);
                var (rparamHash, rparamInfos) = MakeParamHashAndParamInfos(globalContext, funcDecl.TypeParams.Length, funcDecl.Parameters);
                var rtypeArgs = MakeRTypeArgs(0, funcDecl.TypeParams); // NOTICE: global이므로 상위에 type parameter가 없다

                var classPath = classTypeValue.GetRPath_Nested();
                var funcPath = new R.Path.Nested(classPath, rname, rparamHash, rtypeArgs);

                var funcContext = new FuncContext(classTypeValue, retTypeValue, funcDecl.IsStatic, true, funcPath);
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
                var seqFuncDecl = new R.SequenceFuncDecl(decls, funcDecl.Name, !funcDecl.IsStatic, retRType, funcDecl.TypeParams, rparamInfos, bodyResult.Stmt);
                memberFuncsBuilder.Add(seqFuncDecl);
            }

            void AnalyzeNormalFuncDeclElement(S.ClassMemberFuncDecl funcDecl)
            {
                // NOTICE: AnalyzeGlobalNormalFuncDecl와 비슷한 코드                
                var retTypeValue = globalContext.GetTypeValueByTypeExp(funcDecl.RetType);

                var rname = new R.Name.Normal(funcDecl.Name);
                var (rparamHash, rparamInfos) = MakeParamHashAndParamInfos(globalContext, funcDecl.TypeParams.Length, funcDecl.Parameters);
                var rtypeArgs = MakeRTypeArgs(classTypeValue.GetTotalTypeParamCount(), funcDecl.TypeParams);

                var structPath = classTypeValue.GetRPath_Nested();
                var funcPath = new R.Path.Nested(structPath, rname, rparamHash, rtypeArgs);

                var funcContext = new FuncContext(classTypeValue, retTypeValue, funcDecl.IsStatic, false, funcPath);
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

                var normalFuncDecl = new R.NormalFuncDecl(decls, rname, !funcDecl.IsStatic, funcDecl.TypeParams, rparamInfos, bodyResult.Stmt);                
                memberFuncsBuilder.Add(normalFuncDecl);
            }

            void AnalyzeMemberDecl(S.ClassMemberDecl memberDecl)
            {
                switch (memberDecl)
                {
                    case S.ClassMemberVarDecl varDecl:
                        AnalyzeMemberVarDecl(varDecl);
                        break;

                    case S.ClassConstructorDecl constructorDecl:
                        AnalyzeConstructorDecl(constructorDecl);
                        break;

                    case S.ClassMemberFuncDecl funcDecl:
                        AnalyzeMemberFuncDecl(funcDecl);
                        break;

                    default:
                        throw new NotImplementedException();
                }
            }

            void BuildAutomaticConstructor()
            {
                var baseTypeValue = classTypeValue.GetBaseType();
                if (baseTypeValue != null)
                {
                    // throw new NotImplementedException();
                    return;
                }

                var autoConstructor = classTypeValue.GetAutoConstructor();
                if (autoConstructor == null) return;

                var structPath = classTypeValue.GetRPath_Nested();
                var parameters = autoConstructor.GetParameters();
                var paramBuilder = ImmutableArray.CreateBuilder<R.Param>(parameters.Length);
                var stmtBuilder = ImmutableArray.CreateBuilder<R.Stmt>(parameters.Length);
                foreach (var param in parameters)
                {
                    var paramKind = param.Kind switch
                    {
                        M.ParamKind.Normal => R.ParamKind.Normal,
                        M.ParamKind.Ref => R.ParamKind.Ref,
                        M.ParamKind.Params => R.ParamKind.Params,
                        _ => throw new UnreachableCodeException()
                    };

                    var paramTypeValue = globalContext.GetTypeValueByMType(param.Type);
                    var rname = RItemFactory.MakeName(param.Name) as R.Name.Normal;

                    Debug.Assert(rname != null);

                    paramBuilder.Add(new R.Param(paramKind, paramTypeValue.GetRPath(), rname.Value));

                    var structMemberPath = new R.Path.Nested(structPath, rname, R.ParamHash.None, default);
                    stmtBuilder.Add(new R.ExpStmt(new R.AssignExp(new R.ClassMemberLoc(R.ThisLoc.Instance, structMemberPath), new R.LoadExp(new R.LocalVarLoc(rname.Value)))));
                }

                var body = new R.BlockStmt(stmtBuilder.MoveToImmutable());
                constructorsBuilder.Add(new R.ClassConstructorDecl(R.AccessModifier.Public, default, paramBuilder.MoveToImmutable(), null, body));
            }
        }
    }
}