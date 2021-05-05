using Gum.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Gum.IR0Translator.AnalyzeErrorCode;

using S = Gum.Syntax;
using R = Gum.IR0;

namespace Gum.IR0Translator
{
    // 어떤 Exp에서 타입 정보 등을 알아냅니다
    partial class Analyzer
    {
        partial struct StmtAndExpAnalyzer 
        {
            GlobalContext globalContext;
            CallableContext callableContext;
            LocalContext localContext;

            public StmtAndExpAnalyzer(GlobalContext globalContext, CallableContext callableContext, LocalContext localContext)
            {
                this.globalContext = globalContext;
                this.callableContext = callableContext;
                this.localContext = localContext;
            }

            ImmutableArray<TypeValue> GetTypeValues(ImmutableArray<S.TypeExp> typeExps)
            {
                var globalContext = this.globalContext;
                return ImmutableArray.CreateRange(typeExps, typeExp => globalContext.GetTypeValueByTypeExp(typeExp));
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

            [AutoConstructor]
            partial struct LambdaResult
            {
                public bool bCaptureThis { get; }
                public ImmutableArray<string> CaptureLocalVars { get; }
                public LambdaTypeValue TypeValue { get; }
            }

            // Stmt/Exp공통
            LambdaResult AnalyzeLambda(S.ISyntaxNode nodeForErrorReport, S.Stmt body, ImmutableArray<S.LambdaExpParam> parameters)
            {
                // TODO: 리턴 타입은 타입 힌트를 반영해야 한다
                TypeValue? retTypeValue = null;

                // 파라미터는 람다 함수의 지역변수로 취급한다
                var paramInfos = new List<(string Name, TypeValue TypeValue)>();
                foreach (var param in parameters)
                {
                    if (param.Type == null)
                        globalContext.AddFatalError(A9901_NotSupported_LambdaParameterInference, nodeForErrorReport);

                    var paramTypeValue = globalContext.GetTypeValueByTypeExp(param.Type);
                    paramInfos.Add((param.Name, paramTypeValue));
                }

                var newLambdaContext = new LambdaContext(localContext, retTypeValue);
                var newLocalContext = new LocalContext(newLambdaContext);
                var newAnalyzer = new StmtAndExpAnalyzer(globalContext, newLambdaContext, newLocalContext);

                // 람다 파라미터를 지역 변수로 추가한다
                foreach (var paramInfo in paramInfos)
                    newLocalContext.AddLocalVarInfo(paramInfo.Name, paramInfo.TypeValue);

                // 본문 분석
                var bodyResult = newAnalyzer.AnalyzeStmt(body);

                // 성공했으면, 리턴 타입 갱신            
                var capturedLocalVars = newLambdaContext.GetCapturedLocalVars();

                var paramTypes = paramInfos.Select(paramInfo => paramInfo.TypeValue).ToImmutableArray();
                var rparamInfos = paramInfos.Select(paramInfo => new R.ParamInfo(paramInfo.TypeValue.GetRType(), paramInfo.Name)).ToImmutableArray();

                var lambdaDeclId = callableContext.AddLambdaDecl(null, capturedLocalVars, rparamInfos, bodyResult.Stmt);

                var lambdaTypeValue = globalContext.NewLambdaTypeValue(
                    lambdaDeclId,
                    newCallableContext.GetRetTypeValue() ?? VoidTypeValue.Instance,
                    paramTypes
                );

                var captureLocalVarNames = ImmutableArray.CreateRange(capturedLocalVars, c => c.Name);
                var bCaptureThis = newLambdaContext.NeedCaptureThis();

                return new LambdaResult(bCaptureThis, captureLocalVarNames, lambdaTypeValue);
            }

            void CheckParamTypes(S.ISyntaxNode nodeForErrorReport, ImmutableArray<TypeValue> parameters, ImmutableArray<TypeValue> args)
            {
                bool bFatal = false;

                if (parameters.Length != args.Length)
                {
                    context.AddError(A0401_Parameter_MismatchBetweenParamCountAndArgCount, nodeForErrorReport);
                    bFatal = true;
                }

                for (int i = 0; i < parameters.Length; i++)
                {
                    if (!context.IsAssignable(parameters[i], args[i]))
                    {
                        context.AddError(A0402_Parameter_MismatchBetweenParamTypeAndArgType, nodeForErrorReport);
                        bFatal = true;
                    }
                }

                if (bFatal)
                    throw new FatalAnalyzeException();
            }
        }
    }
}