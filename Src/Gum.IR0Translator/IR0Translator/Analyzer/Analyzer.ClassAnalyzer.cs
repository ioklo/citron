using System;
using System.Linq;

using S = Gum.Syntax;
using R = Gum.IR0;
using M = Gum.CompileTime;
using Gum.Infra;
using Gum.Collections;
using Pretune;
using static Gum.IR0Translator.AnalyzeErrorCode;
using System.Diagnostics;
using Gum.Analysis;

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
            ClassSymbol classTypeValue;

            ImmutableArray<R.ClassConstructorDecl>.Builder constructorsBuilder;
            ImmutableArray<R.FuncDecl>.Builder memberFuncsBuilder;
            ImmutableArray<R.ClassMemberVarDecl>.Builder memberVarsBuilder;

            // Entry
            public static void Analyze(GlobalContext globalContext, ITypeContainer typeContainer, S.ClassDecl classDecl, ClassSymbol classTypeValue)
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
                        if (typeValue is ClassSymbol classTypeValue)
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

                classAnalyzer.BuildTrivialConstructor();
                
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
                        var result = baseTypeValue.GetMember(M.Name.Constructor, typeParamCount: 0); // NOTICE: constructor는 타입 파라미터가 없다
                        switch (result)
                        {
                            case MemberQueryResult.Constructors constructorResult:
                                var matchResult = FuncMatcher.Match(globalContext, callableContext, localContext, baseTypeValue.MakeTypeEnv(), constructorResult.ConstructorInfos, baseArgs.Value, default);
                                if (matchResult is FuncMatchResult<IModuleConstructorDecl>.MultipleCandidates)
                                {
                                    globalContext.AddFatalError(A2506_ClassDecl_CannotDecideWhichBaseConstructorUse, nodeForErrorReport);
                                    break;
                                }
                                else if (matchResult is FuncMatchResult<IModuleConstructorDecl>.NotFound)
                                {
                                    globalContext.AddFatalError(A2503_ClassDecl_CannotFindBaseClassConstructor, nodeForErrorReport);
                                    break;
                                }
                                else if (matchResult is FuncMatchResult<IModuleConstructorDecl>.Success matchedConstructor)
                                {
                                    var constructorValue = globalContext.MakeConstructorValue(constructorResult.Outer, matchedConstructor.CallableInfo);

                                    if (!constructorValue.CheckAccess(classTypeValue))
                                        globalContext.AddFatalError(A2504_ClassDecl_CannotAccessBaseClassConstructor, nodeForErrorReport);

                                    return new R.ConstructorBaseCallInfo(constructorValue.GetRPath_Nested().ParamHash, matchedConstructor.Args);
                                }
                                break;

                            case MemberQueryResult.NotFound:
                                globalContext.AddFatalError(A2503_ClassDecl_CannotFindBaseClassConstructor, nodeForErrorReport);
                                break;

                            case MemberQueryResult.Error errorResult:
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
                var seqFuncDecl = new R.SequenceFuncDecl(decls, new R.Name.Normal(funcDecl.Name), !funcDecl.IsStatic, retRType, funcDecl.TypeParams, rparamInfos, bodyResult.Stmt);
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

            void BuildTrivialConstructor()
            {
                static R.Param MakeParam(GlobalContext globalContext, M.Param param)
                {
                    var paramKind = param.Kind switch
                    {
                        M.ParamKind.Default => R.ParamKind.Default,
                        M.ParamKind.Ref => R.ParamKind.Ref,
                        M.ParamKind.Params => R.ParamKind.Params,
                        _ => throw new UnreachableCodeException()
                    };

                    var paramTypeValue = globalContext.GetTypeValueByMType(param.Type);
                    var rname = RItemFactory.MakeName(param.Name);

                    return new R.Param(paramKind, paramTypeValue.GetRPath(), rname);
                }

                // if InternalModuleInfoBuilder decided not to generate TrivialConstructor, pass.
                var trivialConstructor = classTypeValue.GetTrivialConstructor();
                if (trivialConstructor == null) return;

                // to prevent conflict between parameter names, using special name $base- prefix
                // B(int x) / C(int $base_x, int x) : base($base_x) { }

                // 이미 base + current 합쳐진 매개변수
                var parameters = trivialConstructor.GetParameters();
                ImmutableArray<R.Param>.Builder paramBuilder = ImmutableArray.CreateBuilder<R.Param>(parameters.Length);
                R.ConstructorBaseCallInfo? baseCallInfo;
                int baseParamCount = 0;
                
                var baseTypeValue = classTypeValue.GetBaseType();
                if (baseTypeValue != null)
                {
                    // base에서 trivial constructor를 찾는다
                    // class B { int i; bool b; }  // B(int i, bool b)를 찾는다
                    // class C : B { string s; }   // C(int i, bool b, string s)를 찾는다

                    // auto-generated가 아니면, 인자를 (int x, int y)에서 (int y, int x)로 바꿨을 수도 있으므로 자동생성하면 안될 것 같다
                    var baseTrivialConstructor = baseTypeValue.GetTrivialConstructor();

                    // InternalModuleBuilder가 baseTrivialConstructor가 있을때만 trivial constructor 시그니처를 만들었을 것이다.
                    Debug.Assert(baseTrivialConstructor != null);

                    var baseParameters = baseTrivialConstructor.GetParameters();
                    var baseParamHashEntriesBuilder = ImmutableArray.CreateBuilder<R.ParamHashEntry>(baseParameters.Length);
                    var baseConstructorArgs = ImmutableArray.CreateBuilder<R.Argument>(baseParameters.Length);

                    foreach (var param in baseParameters)
                    {
                        var rparam = MakeParam(globalContext, param);
                        paramBuilder.Add(rparam);

                        baseParamHashEntriesBuilder.Add(new R.ParamHashEntry(rparam.Kind, rparam.Type));

                        R.Argument arg = rparam.Kind switch
                        {
                            R.ParamKind.Default => new R.Argument.Normal(new R.LoadExp(new R.LocalVarLoc(rparam.Name))),
                            R.ParamKind.Params => throw new UnreachableCodeException(),
                            R.ParamKind.Ref => new R.Argument.Ref(new R.LocalVarLoc(rparam.Name)),
                            _ => throw new UnreachableCodeException()
                        };

                        baseConstructorArgs.Add(arg);
                    }

                    baseCallInfo = new R.ConstructorBaseCallInfo(
                        new R.ParamHash(0, baseParamHashEntriesBuilder.MoveToImmutable()),
                        baseConstructorArgs.MoveToImmutable()
                    );

                    baseParamCount = baseParameters.Length;
                }
                else
                {
                    baseCallInfo = null;
                    baseParamCount = 0;
                }
                
                var classPath = classTypeValue.GetRPath_Nested();
                var stmtBuilder = ImmutableArray.CreateBuilder<R.Stmt>(parameters.Length - baseParamCount);
                for(int i = baseParamCount; i < parameters.Length; i++)
                {
                    var param = parameters[i];

                    var rparam = MakeParam(globalContext, param);
                    paramBuilder.Add(rparam);

                    var memberPath = new R.Path.Nested(classPath, rparam.Name, R.ParamHash.None, default);
                    stmtBuilder.Add(new R.ExpStmt(new R.AssignExp(new R.ClassMemberLoc(R.ThisLoc.Instance, memberPath), new R.LoadExp(new R.LocalVarLoc(rparam.Name)))));
                }

                var body = new R.BlockStmt(stmtBuilder.MoveToImmutable());

                constructorsBuilder.Add(new R.ClassConstructorDecl(
                    R.AccessModifier.Public, default, paramBuilder.MoveToImmutable(),
                    baseCallInfo,
                    body
                ));
            }
        }
    }
}
