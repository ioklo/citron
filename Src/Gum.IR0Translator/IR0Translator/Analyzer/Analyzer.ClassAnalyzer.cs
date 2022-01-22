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
            ClassSymbol classSymbol;

            ImmutableArray<R.ClassConstructorDecl>.Builder constructorsBuilder;
            ImmutableArray<R.FuncDecl>.Builder memberFuncsBuilder;
            ImmutableArray<R.ClassMemberVarDecl>.Builder memberVarsBuilder;

            // Entry
            public static void Analyze(GlobalContext globalContext, ITypeContainer typeContainer, ModuleSymbolId outerId, S.ClassDecl classDecl)
            {
                var classSymbol = globalContext.LoadOpenSymbol<ClassSymbol>(outerId, classDecl.Name, classDecl.TypeParams, default);
                Debug.Assert(classSymbol != null);

                var constructorsBuilder = ImmutableArray.CreateBuilder<R.ClassConstructorDecl>();
                var memberFuncsBuilder = ImmutableArray.CreateBuilder<R.FuncDecl>();
                var memberVarsBuilder = ImmutableArray.CreateBuilder<R.ClassMemberVarDecl>();
                
                var classAnalyzer = new ClassAnalyzer(globalContext, typeContainer, classDecl, classSymbol,
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
                        var typeValue = globalContext.GetSymbolByTypeExp(baseType);
                        if (typeValue is ClassSymbol classSymbol)
                            return classSymbol.MakeRPath();
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
                var varTypeValue = globalContext.GetSymbolByTypeExp(varDecl.VarType);
                var rtype = varTypeValue.MakeRPath();

                R.AccessModifier accessModifier = AnalyzeAccessModifier(varDecl.AccessModifier, varDecl);
                memberVarsBuilder.Add(new R.ClassMemberVarDecl(accessModifier, rtype, varDecl.VarNames));
            }

            R.ConstructorBaseCallInfo? HandleConstructorDeclBaseArgs(ICallableContext callableContext, LocalContext localContext, ImmutableArray<S.Argument>? baseArgs, S.ISyntaxNode nodeForErrorReport)
            {
                if (baseArgs == null)
                {
                    var baseTypeValue = classSymbol.GetBaseClass();
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

                    if (!constructor.CheckAccess(classSymbol))
                        globalContext.AddFatalError(A2504_ClassDecl_CannotAccessBaseClassConstructor, nodeForErrorReport);

                    // no argument
                    return new R.ConstructorBaseCallInfo(R.ParamHash.None, default);
                }
                else
                {
                    // 인자가 있는 상황
                    var baseClass = classSymbol.GetBaseClass();

                    // 인자가 있는데 부모 클래스가 없으면,
                    if (baseClass == null)
                        globalContext.AddFatalError(A2505_ClassDecl_TryCallBaseConstructorWithoutBaseClass, nodeForErrorReport);

                    var baseConstructors = baseClass.GetConstructors();

                    if (baseConstructors.Length == 0)
                        globalContext.AddFatalError(A2503_ClassDecl_CannotFindBaseClassConstructor, nodeForErrorReport);

                    var matchResult = FuncMatcher.MatchIndex(globalContext, callableContext, localContext, baseClass.GetTypeEnv(),
                        baseConstructors, baseArgs.Value, default);

                    switch (matchResult)
                    {
                        case FuncMatchIndexResult.MultipleCandidates:
                            globalContext.AddFatalError(A2506_ClassDecl_CannotDecideWhichBaseConstructorUse, nodeForErrorReport);
                            break;

                        case FuncMatchIndexResult.NotFound:
                            globalContext.AddFatalError(A2503_ClassDecl_CannotFindBaseClassConstructor, nodeForErrorReport);
                            break;

                        case FuncMatchIndexResult.Success match:
                            var baseConstructor = baseConstructors[match.Index];
                            Debug.Assert(match.TypeArgs.Length == 0);

                            if (!baseConstructor.CheckAccess(classSymbol))
                                globalContext.AddFatalError(A2504_ClassDecl_CannotAccessBaseClassConstructor, nodeForErrorReport);

                            return new R.ConstructorBaseCallInfo(baseConstructor.MakeRPath().ParamHash, match.Args);
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

                var constructorPath = new R.Path.Nested(classSymbol.MakeRPath(), R.Name.Constructor.Instance, rparamHash, default);
                var constructorContext = new ClassConstructorContext(constructorPath, classSymbol);
                var localContext = new LocalContext();

                // 새로 만든 컨텍스트에 파라미터 순서대로 추가
                foreach (var param in constructorDecl.Parameters)
                {
                    var paramTypeValue = globalContext.GetSymbolByTypeExp(param.Type);
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
                var retTypeValue = globalContext.GetSymbolByTypeExp(funcDecl.RetType);
                var rname = new R.Name.Normal(funcDecl.Name);
                var (rparamHash, rparamInfos) = MakeParamHashAndParamInfos(globalContext, funcDecl.TypeParams.Length, funcDecl.Parameters);
                var rtypeArgs = MakeRTypeArgs(0, funcDecl.TypeParams); // NOTICE: global이므로 상위에 type parameter가 없다

                var classPath = classSymbol.MakeRPath();
                var funcPath = new R.Path.Nested(classPath, rname, rparamHash, rtypeArgs);

                var funcContext = new FuncContext(classSymbol, retTypeValue, funcDecl.IsStatic, true, funcPath);
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

            void AnalyzeNormalFuncDeclElement(S.ClassMemberFuncDecl funcDecl)
            {
                // NOTICE: AnalyzeGlobalNormalFuncDecl와 비슷한 코드                
                var retTypeValue = globalContext.GetSymbolByTypeExp(funcDecl.RetType);

                var rname = new R.Name.Normal(funcDecl.Name);
                var (rparamHash, rparamInfos) = MakeParamHashAndParamInfos(globalContext, funcDecl.TypeParams.Length, funcDecl.Parameters);
                var rtypeArgs = MakeRTypeArgs(classSymbol.GetTotalTypeParamCount(), funcDecl.TypeParams);

                globalContext.LoadOpenSymbol(classId,)

                var classPath = classSymbol.MakeRPath();
                var funcPath = new R.Path.Nested(classPath, rname, rparamHash, rtypeArgs);

                var funcContext = new FuncContext(classSymbol, retTypeValue, funcDecl.IsStatic, false, funcPath);
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
                // if InternalModuleInfoBuilder decided not to generate TrivialConstructor, pass.
                var trivialConstructor = classSymbol.GetTrivialConstructor();
                if (trivialConstructor == null) return;

                // to prevent conflict between parameter names, using special name $base- prefix
                // B(int x) / C(int $base_x, int x) : base($base_x) { }

                // 이미 base + current 합쳐진 매개변수
                var parameterCount = trivialConstructor.GetParameterCount();
                ImmutableArray<R.Param>.Builder paramBuilder = ImmutableArray.CreateBuilder<R.Param>(parameters.Length);
                R.ConstructorBaseCallInfo? baseCallInfo;
                int baseParamCount = 0;
                
                var baseTypeValue = classSymbol.GetBaseClass();
                if (baseTypeValue != null)
                {
                    // base에서 trivial constructor를 찾는다
                    // class B { int i; bool b; }  // B(int i, bool b)를 찾는다
                    // class C : B { string s; }   // C(int i, bool b, string s)를 찾는다

                    // auto-generated가 아니면, 인자를 (int x, int y)에서 (int y, int x)로 바꿨을 수도 있으므로 자동생성하면 안될 것 같다
                    var baseTrivialConstructor = baseTypeValue.GetTrivialConstructor();

                    // InternalModuleBuilder가 baseTrivialConstructor가 있을때만 trivial constructor 시그니처를 만들었을 것이다.
                    Debug.Assert(baseTrivialConstructor != null);

                    baseParamCount = baseTrivialConstructor.GetParameterCount();
                    var baseConstructorPath = baseTrivialConstructor.GetSymbolId().MakeRPath() as R.Path.Nested;
                    Debug.Assert(baseConstructorPath != null);

                    var baseConstructorArgs = ImmutableArray.CreateBuilder<R.Argument>(baseParamCount);
                    for(int i = 0; i < baseParamCount; i++)
                    {
                        var baseParam = baseTrivialConstructor.GetParameter(i);
                        var rbaseParam = baseParam.MakeRParam();
                        paramBuilder.Add(rbaseParam);

                        R.Argument arg = baseParam.Kind switch
                        {
                            FuncParameterKind.Default => new R.Argument.Normal(new R.LoadExp(new R.LocalVarLoc(rbaseParam.Name))),
                            FuncParameterKind.Params => throw new UnreachableCodeException(),
                            FuncParameterKind.Ref => new R.Argument.Ref(new R.LocalVarLoc(rbaseParam.Name)),
                            _ => throw new UnreachableCodeException()
                        };
                        baseConstructorArgs.Add(arg);
                    }

                    baseCallInfo = new R.ConstructorBaseCallInfo(
                        baseConstructorPath.ParamHash,
                        baseConstructorArgs.MoveToImmutable()
                    );
                }
                else
                {
                    baseCallInfo = null;
                    baseParamCount = 0;
                }

                var classPath = classSymbol.MakeRPath() as R.Path.Normal;
                Debug.Assert(classPath != null);

                var stmtBuilder = ImmutableArray.CreateBuilder<R.Stmt>(parameterCount - baseParamCount);
                for(int i = baseParamCount; i < parameterCount; i++)
                {
                    var param = trivialConstructor.GetParameter(i);
                    var rparam = param.MakeRParam();
                    paramBuilder.Add(rparam);
                    
                    var memberVar = classSymbol.GetMemberVar(param.Name);
                    Debug.Assert(memberVar != null);

                    stmtBuilder.Add(new R.ExpStmt(new R.AssignExp(new R.ClassMemberLoc(new R.ThisLoc(classSymbol), memberVar), new R.LoadExp(new R.LocalVarLoc(rparam.Name)))));
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
