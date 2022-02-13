using Gum.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Gum.IR0Translator.AnalyzeErrorCode;

using S = Gum.Syntax;
using R = Gum.IR0;
using Pretune;
using Gum.Infra;
using Citron.Analysis;

namespace Gum.IR0Translator
{
    // 어떤 Exp에서 타입 정보 등을 알아냅니다
    partial class Analyzer
    {
        partial struct StmtAndExpAnalyzer 
        {
            GlobalContext globalContext;
            ICallableContext callableContext;
            LocalContext localContext;

            public StmtAndExpAnalyzer(GlobalContext globalContext, ICallableContext callableContext, LocalContext localContext)
            {
                this.globalContext = globalContext;
                this.callableContext = callableContext;
                this.localContext = localContext;
            }
            
            ImmutableArray<ITypeSymbol> GetTypeValues(ImmutableArray<S.TypeExp> typeExps)
            {
                var builder = ImmutableArray.CreateBuilder<ITypeSymbol>(typeExps.Length);

                foreach (var typeExp in typeExps)
                {
                    var typeValue = globalContext.GetSymbolByTypeExp(typeExp);
                    builder.Add(typeValue);
                }

                return builder.MoveToImmutable();
            }

            StmtAndExpAnalyzer NewAnalyzer()
            {
                var newLocalContext = localContext.NewLocalContext();
                return new StmtAndExpAnalyzer(globalContext, callableContext, newLocalContext);
            }            

            StmtAndExpAnalyzer NewAnalyzerWithLoop()
            {
                var newLocalContext = localContext.NewLocalContextWithLoop();
                return new StmtAndExpAnalyzer(globalContext, callableContext, newLocalContext);
            }

            // var x = 3, y = ref i; 라면 
            R.LocalVarDecl AnalyzeLocalVarDecl(S.VarDecl varDecl)
            {
                var varDeclAnalyzer = new VarDeclElemAnalyzer(globalContext, callableContext, localContext);
                var declType = globalContext.GetSymbolByTypeExp(varDecl.Type);

                var relems = new List<R.VarDeclElement>();
                foreach (var elem in varDecl.Elems)
                {
                    if (localContext.DoesLocalVarNameExistInScope(elem.VarName))
                        globalContext.AddFatalError(A0103_VarDecl_LocalVarNameShouldBeUniqueWithinScope, elem);

                    var result = varDeclAnalyzer.AnalyzeVarDeclElement(bLocal: true, elem, varDecl.IsRef, declType);

                    // varDecl.IsRef는 syntax에서 체크한 것이므로, syntax에서 ref가 아니더라도 ref일 수 있으므로 result.Elem으로 검사를 해야한다.
                    localContext.AddLocalVarInfo(result.Elem is R.VarDeclElement.Ref, result.TypeSymbol, elem.VarName);
                    relems.Add(result.Elem);
                }

                return new R.LocalVarDecl(relems.ToImmutableArray());
            }

            (LambdaSymbol Lambda, R.Stmt Body) AnalyzeLambda(ITypeSymbol? retType, ImmutableArray<S.LambdaExpParam> sparams, S.Stmt body, S.ISyntaxNode nodeForErrorReport)
            {
                // TODO: 리턴 타입은 타입 힌트를 반영해야 한다
                // 파라미터는 람다 함수의 지역변수로 취급한다                
                var newCapturedContext = new CapturedContext(callableContext.GetThisType(), localContext, retType);

                var newLocalContext = new LocalContext();
                // 람다 파라미터를 지역 변수로 추가한다
                var funcParameters = ImmutableArray.CreateBuilder<FuncParameter>(sparams.Length);
                foreach (var sparam in sparams)
                {
                    if (sparam.Type == null)
                        globalContext.AddFatalError(A9901_NotSupported_LambdaParameterInference, nodeForErrorReport);

                    var paramType = globalContext.GetSymbolByTypeExp(sparam.Type);

                    var paramKind = sparam.ParamKind switch
                    {
                        S.FuncParamKind.Normal => FuncParameterKind.Default,
                        S.FuncParamKind.Params => FuncParameterKind.Params,
                        S.FuncParamKind.Ref => FuncParameterKind.Ref,
                        _ => throw new UnreachableCodeException()
                    };

                    funcParameters.Add(new FuncParameter(paramKind, paramType, new M.Name.Normal(sparam.Name)));
                    newLocalContext.AddLocalVarInfo(sparam.ParamKind == S.FuncParamKind.Ref, paramType, sparam.Name);
                }

                var newAnalyzer = new StmtAndExpAnalyzer(globalContext, newCapturedContext, newLocalContext);

                // 본문 분석
                var bodyResult = newAnalyzer.AnalyzeStmt(body);

                // TODO: need capture this확인해서 this 넣기
                // var bCaptureThis = newLambdaContext.NeedCaptureThis();
                ITypeSymbol? capturedThisType = null;

                var funcNode = callableContext.GetThisNode();

                var capturedContextName = callableContext.NewAnonymousName();
                var lambdaDeclHolder = new Holder<LambdaDeclSymbol>();

                var capturedLocalVars = newCapturedContext.GetCapturedLocalVars();
                int memberVarCount = capturedLocalVars.Length + (capturedThisType != null ? 1 : 0);
                var memberVarsBuilder = ImmutableArray.CreateBuilder<LambdaMemberVarDeclSymbol>(memberVarCount);
                if (capturedThisType != null)
                {
                    var memberVar = new LambdaMemberVarDeclSymbol(lambdaDeclHolder, capturedThisType, M.Name.CapturedThis);
                    memberVarsBuilder.Add(memberVar);
                }

                foreach (var capturedLocalVar in capturedLocalVars)
                {
                    var memberVar = new LambdaMemberVarDeclSymbol(lambdaDeclHolder, capturedLocalVar.DeclType, capturedLocalVar.VarName);
                    memberVarsBuilder.Add(memberVar);
                }

                var lambdaDecl = new LambdaDeclSymbol(funcNode.GetDeclSymbolNode(), capturedContextName, newCapturedContext.GetReturn(), funcParameters.MoveToImmutable(), memberVarsBuilder.MoveToImmutable());
                lambdaDeclHolder.SetValue(lambdaDecl);

                // Stmt분석시점에 추가되는 Declaration
                callableContext.AddLambdaDecl(lambdaDecl);
                var context = globalContext.MakeLambda(funcNode, lambdaDecl);

                return (context, bodyResult.Stmt);
            }
        }
    }
}