using Citron.Collections;
using Citron.Symbol;
using S = Citron.Syntax;
using R = Citron.IR0;

using static Citron.Analysis.SyntaxAnalysisErrorCode;
using System;
using Citron.Infra;
using System.Diagnostics;

namespace Citron.Analysis
{
    struct LambdaVisitor
    {
        // retType이 null이면 아직 안정해졌다는 뜻이다
        public static TranslationResult<(LambdaSymbol Lambda, ImmutableArray<R.Exp> Args)> Translate(IType? retType, ImmutableArray<S.LambdaExpParam> paramSyntaxes, ImmutableArray<S.Stmt> bodySyntaxes, ScopeContext context, S.ISyntaxNode nodeForErrorReport)
        {
            TranslationResult<(LambdaSymbol, ImmutableArray<R.Exp>)> Error() => TranslationResult.Error<(LambdaSymbol, ImmutableArray<R.Exp>)>();

            // 람다를 분석합니다
            // [int x = x](int p) => { return 3; }

            // 파라미터는 람다 함수의 지역변수로 취급한다
            // var newLambdaBodyContext = bodyContext.NewLambdaBodyContext(localContext); // new FuncContext(lambdaDeclHolder, bodyContext.GetThisType(), bSeqFunc: false, localContext);

            // 람다 관련 정보는 여기서 수집한다

            FuncReturn? ret = (retType == null) ? null : new FuncReturn(retType);
            var parameters = MakeParameters(paramSyntaxes, context);

            // Lambda를 만들고 context 인스턴스 안에 저장한다
            // DeclSymbol tree로의 Commit은 함수 백트래킹이 다 끝났을 때 (그냥 Translation이 끝났을때 해도 될거 같다)
            var (newContext, lambda) = context.MakeLambdaBodyContext(ret, parameters); // 중첩된 bodyContext를 만들고, 새 scopeContext도 만든다

            // 람다 파라미터(int p)를 지역 변수로 추가한다
            foreach (var paramSyntax in paramSyntaxes)
            {
                // TODO: 파라미터 타입은 타입 힌트를 반영해야 한다, ex) func<void, int, int> f = (x, y) => { } 일때, x, y는 int
                if (paramSyntax.Type == null)
                {
                    context.AddFatalError(A9901_NotSupported_LambdaParameterInference, nodeForErrorReport);
                    return Error();
                }

                var paramType = context.MakeType(paramSyntax.Type);
                newContext.AddLocalVarInfo(paramType, new Name.Normal(paramSyntax.Name));
            }

            var bodyStmtsResult = StmtVisitor.TranslateBody(bodySyntaxes, newContext);
            if (!bodyStmtsResult.IsValid(out var bodyStmts))
                return Error();

            context.AddBody(lambda.GetDeclSymbol(), bodyStmts);

            var args = newContext.MakeLambdaArgs();
            return TranslationResult.Valid<(LambdaSymbol, ImmutableArray<R.Exp>)>((lambda, args));
        }

        static ImmutableArray<FuncParameter> MakeParameters(ImmutableArray<S.LambdaExpParam> paramSyntaxes, ScopeContext context)
        {
            var paramsBuilder = ImmutableArray.CreateBuilder<FuncParameter>(paramSyntaxes.Length);
            foreach (var paramSyntax in paramSyntaxes)
            {
                var paramKind = paramSyntax.ParamKind switch
                {
                    S.FuncParamKind.Normal => FuncParameterKind.Default,
                    S.FuncParamKind.Params => FuncParameterKind.Params,
                    _ => throw new UnreachableException()
                };

                // 파라미터에 Type이 명시되어있지 않으면 hintType기반으로 inference 해야 한다.
                if (paramSyntax.Type == null)
                    throw new NotImplementedException();

                var paramTypeSymbol = context.MakeType(paramSyntax.Type);
                var param = new FuncParameter(paramKind, paramTypeSymbol, new Name.Normal(paramSyntax.Name));
                paramsBuilder.Add(param);
            }

            return paramsBuilder.MoveToImmutable();
        }
    }
}
