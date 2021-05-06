using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using S = Gum.Syntax;
using R = Gum.IR0;
using Gum.Collections;

namespace Gum.IR0Translator
{
    partial class Analyzer
    {
        // TypeDecl등을 분석하는
        partial struct DeclAnalyzer
        {
            GlobalContext globalContext;
            DeclContext declContext;

            public void AnalyzeGlobalFuncDecl(S.GlobalFuncDecl funcDecl)
            {
                var retTypeValue = globalContext.GetTypeValueByTypeExp(funcDecl.RetType);
                var funcContext = new FuncContext(retTypeValue, false);
                var localContext = new LocalContext(funcContext);
                var analyzer = new StmtAndExpAnalyzer(globalContext, funcContext, localContext);

                if (0 < funcDecl.TypeParams.Length || funcDecl.ParamInfo.VariadicParamIndex != null)
                    throw new NotImplementedException();

                // 파라미터 순서대로 추가
                foreach (var param in funcDecl.ParamInfo.Parameters)
                {
                    var paramTypeValue = globalContext.GetTypeValueByTypeExp(param.Type);
                    localContext.AddLocalVarInfo(param.Name, paramTypeValue);
                }

                var bodyResult = analyzer.AnalyzeStmt(funcDecl.Body);

                var thisGlobalContext = this.globalContext;

                // TODO: Body가 실제로 리턴을 제대로 하는지 확인해야 한다
                var parameters = funcDecl.ParamInfo.Parameters.Select(param =>
                {
                    var paramTypeValue = thisGlobalContext.GetTypeValueByTypeExp(param.Type);
                    return new R.ParamInfo(paramTypeValue.GetRType(), param.Name);

                }).ToImmutableArray();

                // 
                var lambdaDecls = funcContext.GetDecls();
                declContext.AddDecl(new R.NormalFuncDecl(lambdaDecls, funcDecl.Name, false, funcDecl.TypeParams, parameters, bodyResult.Stmt));
            }

            public void AnalyzeFuncDecl(S.FuncDecl funcDecl)
            {
                if (!funcDecl.IsSequence)
                    AnalyzeNormalFuncDecl(funcDecl);
                else
                    AnalyzeSequenceFuncDecl(funcDecl);

                context.ExecInFuncScope(funcDecl, () =>
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

                    if (funcDecl.IsSequence)
                    {
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

                        context.AddSequenceFuncDecl(retRType, false, funcDecl.TypeParams, rparamInfos, bodyResult.Stmt);
                    }
                    else
                    {
                        // TODO: Body가 실제로 리턴을 제대로 하는지 확인해야 한다
                        var parameters = funcDecl.ParamInfo.Parameters.Select(param =>
                        {
                            var paramTypeValue = context.GetTypeValueByTypeExp(param.Type);
                            return new R.ParamInfo(paramTypeValue.GetRType(), param.Name);
                        }).ToImmutableArray();

                        context.AddNormalFuncDecl(funcDecl.Name, bThisCall: false, funcDecl.TypeParams, parameters, bodyResult.Stmt);
                    }
                });
            }

            public void AnalyzeTypeDecl(S.TypeDecl typeDecl)
            {
                throw new NotImplementedException();
            }
        }
    }
}
