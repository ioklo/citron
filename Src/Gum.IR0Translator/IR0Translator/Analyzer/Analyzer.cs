using System;
using System.Collections.Generic;
using Gum.Collections;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net;
using System.Text;
using M = Gum.CompileTime;
using Gum.Infra;
using Pretune;
using static Gum.IR0Translator.AnalyzeErrorCode;

using S = Gum.Syntax;
using R = Gum.IR0;

namespace Gum.IR0Translator
{
    // Root Analyzer
    partial class Analyzer
    {
        public static R.Script Analyze(
            S.Script script,
            R.ModuleName moduleName,
            ItemValueFactory itemValueFactory,
            GlobalItemValueFactory globalItemValueFactory,
            TypeExpInfoService typeExpTypeValueService,
            IErrorCollector errorCollector)
        {
            var globalContext = new GlobalContext(itemValueFactory, globalItemValueFactory, typeExpTypeValueService, errorCollector);            
            var analyzer = new Analyzer(globalContext);

            var rootContext = new RootContext(moduleName, itemValueFactory);
            var localContext = new LocalContext(rootContext);

            var rootAnalyzer = new RootAnalyzer(rootContext);
            RootAnalyzer.Analyze(globalContext, rootContext, localContext);

            // pass1, pass2
            var pass1 = new CollectingGlobalVarPass(analyzer); // 말이 틀렸다. TopLevelStmt를 여기서 분석하고 있다..
            IR0Translator.Misc.VisitScript(script, pass1);

            var pass2 = new TypeCheckingAndTranslatingPass(analyzer);
            IR0Translator.Misc.VisitScript(script, pass2);

            // 5. 각 func body를 분석한다 (4에서 얻게되는 글로벌 변수 정보가 필요하다)
            return rootContext.MakeScript();
        }
        
        public void AnalyzeNormalFuncDecl(S.FuncDecl funcDecl)
        {
            var retTypeValue = globalContext.GetTypeValueByTypeExp(funcDecl.RetType);
            var funcContext = new FuncContext(retTypeValue, false);
            var newContext = new LocalContext(funcContext);
            var newStmtAnalyzer = new StmtAndExpAnalyzer(newContext);
            
                if (0 < funcDecl.TypeParams.Length || funcDecl.ParamInfo.VariadicParamIndex != null)
                    throw new NotImplementedException();

                // 파라미터 순서대로 추가
                foreach (var param in funcDecl.ParamInfo.Parameters)
                {
                    var paramTypeValue = context.GetTypeValueByTypeExp(param.Type);
                    context.AddLocalVarInfo(param.Name, paramTypeValue);
                }

                var bodyResult = newStmtAnalyzer.AnalyzeStmt(funcDecl.Body);
                
                // TODO: Body가 실제로 리턴을 제대로 하는지 확인해야 한다
                var parameters = funcDecl.ParamInfo.Parameters.Select(param =>
                {
                    var paramTypeValue = context.GetTypeValueByTypeExp(param.Type);
                    return new R.ParamInfo(paramTypeValue.GetRType(), param.Name);
                }).ToImmutableArray();

            // 
            var lambdaDecls = funcContext.GetLambdaDecls();
            context.AddNormalFuncDecl(lambdaDecls, funcDecl.Name, bThisCall: false, funcDecl.TypeParams, parameters, bodyResult.Stmt);
        }

        public void AnalyzeSequenceFuncDecl(S.FuncDecl funcDecl)
        {
            var (bodyResult, retRType, rparamInfos) = context.ExecInFuncScope(funcDecl, () =>
            {
                if (0 < funcDecl.TypeParams.Length || funcDecl.ParamInfo.VariadicParamIndex != null)
                    throw new NotImplementedException();

                // 파라미터 순서대로 추가
                foreach (var param in funcDecl.ParamInfo.Parameters)
                {
                    var paramTypeValue = context.GetTypeValueByTypeExp(param.Type);
                    context.AddLocalVarInfo(param.Name, paramTypeValue);
                }

                var bodyResult = AnalyzeStmt(funcDecl.Body);

                
                // TODO: Body가 실제로 리턴을 제대로 하는지 확인해야 한다
                var retTypeValue = context.GetRetTypeValue();
                Debug.Assert(retTypeValue != null, "문법상 Sequence 함수의 retValue가 없을수 없습니다");

                var retRType = retTypeValue.GetRType();
                var parameters = funcDecl.ParamInfo.Parameters.Select(param => param.Name).ToImmutableArray();

                var rparamInfos = ImmutableArray.CreateRange(funcDecl.ParamInfo.Parameters, param =>
                {
                    var typeValue = context.GetTypeValueByTypeExp(param.Type);
                    var rtype = typeValue.GetRType();

                    return new R.ParamInfo(rtype, param.Name);
                });

                return (bodyResult, retRType, rparamInfos);
            });

            context.AddSequenceFuncDecl(retRType, false, funcDecl.TypeParams, rparamInfos, bodyResult.Stmt);
        }

        
    }
}
