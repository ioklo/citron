using System;

using Citron.Collections;
using Citron.Symbol;
using S = Citron.Syntax;
using R = Citron.IR0;
using Citron.Infra;
using Pretune;
using System.Diagnostics;
using System.Collections.Generic;

namespace Citron.Analysis;

partial struct FuncsMatcher
{   
    record struct MatchParamArgResult(bool IsExactMatch, R.Argument Argument);
    static MatchParamArgResult? MatchParamArg(IType paramType, S.Argument sarg, ScopeContext context)
    {
        switch (sarg)
        {
            case S.Argument.Normal snormalArg:
                {
                    var argResult = ExpIR0ExpTranslator.Translate(snormalArg.Exp, context, paramType);
                    if (!argResult.IsValid(out var argExpResult))
                        return null;

                    if (BodyMisc.TypeEquals(paramType, argExpResult.ExpType))
                    {
                        // exact
                        return new MatchParamArgResult(IsExactMatch: true, new R.Argument.Normal(argExpResult.Exp));
                    }
                    else
                    {
                        var castExp = BodyMisc.TryCastExp_Exp(argExpResult.Exp, argExpResult.ExpType, paramType, context);
                        if (castExp == null) return null;

                        return new MatchParamArgResult(IsExactMatch: false, new R.Argument.Normal(castExp));
                    }
                }

            case S.Argument.Params:
                throw new NotImplementedException();

            default:
                throw new UnreachableException();
        }
    }
    
    record struct MatchFuncResult(bool IsExactMatch, ImmutableArray<IType> TypeArgs, ImmutableArray<R.Argument> RArgs);
    static MatchFuncResult? MatchFunc<TFuncDeclSymbol>(TFuncDeclSymbol decl, TypeEnv outerTypeEnv, ImmutableArray<IType> partialTypeArgs, ImmutableArray<S.Argument> sargs, ScopeContext context)
        where TFuncDeclSymbol : IFuncDeclSymbol
    {
        TypeEnv typeEnv;

        // 아직 type inference를 구현하지 않았다
        if (decl.GetTypeParamCount() != partialTypeArgs.Length)
            throw new NotImplementedException(); // TODO: typeInference
        else        
            typeEnv = outerTypeEnv.AddTypeArgs(partialTypeArgs);

        // 그냥 여기다가 일단 해본다
        var rargsBuilder = ImmutableArray.CreateBuilder<R.Argument>();

        if (!decl.IsLastParameterVariadic())
        {
            // variadic이 아닌경우
            int paramCount = decl.GetParameterCount();
            if (paramCount != sargs.Length)
                return null;

            bool bExactMatch = true;

            for (int i = 0; i < paramCount; i++)
            {
                var param = decl.GetParameter(i);
                var sarg = sargs[i];

                var paramType = param.Type.Apply(typeEnv);

                var matchParamArgResult = MatchParamArg(paramType, sarg, context);
                if (matchParamArgResult == null)
                    return null;

                if (!matchParamArgResult.Value.IsExactMatch)
                    bExactMatch = false;

                rargsBuilder.Add(matchParamArgResult.Value.Argument);
            }

            // 별탈없이 끝났으면
            return new MatchFuncResult(bExactMatch, partialTypeArgs, rargsBuilder.ToImmutable());
        }
        else
        {
            throw new NotImplementedException();
        }
    }

    // 결과
    public static (TFuncSymbol Symbol, ImmutableArray<R.Argument> Args)? Match<TFuncDeclSymbol, TFuncSymbol>(        
        IFuncs<TFuncDeclSymbol, TFuncSymbol> candidates,
        ImmutableArray<S.Argument> sargs,
        ScopeContext context)
        where TFuncDeclSymbol : IFuncDeclSymbol
        where TFuncSymbol : IFuncSymbol
    {
        (MatchFuncResult Result, int Index, ScopeContext Context)? exactMatch = null;
        var matches = new List<(MatchFuncResult Result, int Index, ScopeContext Context)>();

        int candidateCount = candidates.GetCount();
        for(int i = 0; i < candidateCount; i++)
        {
            var cloneContext = new CloneContext();
            var newContext = cloneContext.GetClone(context);

            var decl = candidates.GetDecl(i);
            var outerTypeEnv = candidates.GetOuterTypeEnv(i);
            var partialTypeArgs = candidates.GetPartialTypeArgs();

            var matchFuncResult = MatchFunc(decl, outerTypeEnv, partialTypeArgs, sargs, newContext);
            if (matchFuncResult == null) continue;

            if (matchFuncResult.Value.IsExactMatch)
            {   
                if (exactMatch != null)
                    throw new NotImplementedException(); // 두개의 exact match 에러

                exactMatch = (matchFuncResult.Value, i, newContext);
            }
            else
            {
                matches.Add((matchFuncResult.Value, i, newContext));
            }
        }

        (MatchFuncResult Result, int Index, ScopeContext Context) finalMatch;

        if (exactMatch.HasValue)
            finalMatch = exactMatch.Value;
        else if (matches.Count == 1)
            finalMatch = matches[0];
        else if (matches.Count == 0)
            return null;
        else
            throw new NotImplementedException(); // TODO: 매치가 모호합니다

        var symbol = candidates.MakeSymbol(finalMatch.Index, finalMatch.Result.TypeArgs, finalMatch.Context);

        // context 최종 업데이트
        var updateContext = new UpdateContext();
        updateContext.Update(context, finalMatch.Context);

        return (symbol, finalMatch.Result.RArgs);
    }
}
