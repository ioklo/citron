using System.Collections.Generic;
using System.Diagnostics;

using Citron.Collections;
using Citron.Symbol;
using S = Citron.Syntax;
using R = Citron.IR0;

using static Citron.Analysis.SyntaxAnalysisErrorCode;

namespace Citron.Analysis;

abstract record FuncMatcherResult
{
    record ExactMatch(ImmutableArray<R.Exp> Args) : FuncMatcherResult;
    record Match(ImmutableArray<R.Exp> Args) : FuncMatcherResult;
    record NotMatched() : FuncMatcherResult;
}

struct ArgumentEnumerator
{
    ScopeContext context;
    TypeEnv outerTypeEnv;

    ImmutableArray<S.Argument> argsSyntax;
    int nextArgsSyntaxIndex;
    int nextExpandedArgIndex;
    List<R.Exp> expandedArgs;

    public bool HasNext()
    {
        if (nextExpandedArgIndex < expandedArgs.Count)
            return true;

        if (nextArgsSyntaxIndex < argsSyntax.Length)
            return true;

        return false;
    }

    TranslationResult<R.Exp?> GetNext(IType paramType)
    {
        // expanded된 argument가 있는 경우
        if (nextExpandedArgIndex < expandedArgs.Count)
        {
            var expandedArg = expandedArgs[nextExpandedArgIndex];
            nextExpandedArgIndex++;
            return TranslationResult.Valid<R.Exp?>(expandedArg);
        }
        else if (nextArgsSyntaxIndex < argsSyntax.Length) // 새로 syntax에서 가져올 수 있으면
        {
            var argSyntax = argsSyntax[nextArgsSyntaxIndex];
            nextArgsSyntaxIndex++;

            // 일반 argument라면
            if (argSyntax is S.Argument.Normal normalArgSyntax)
            {
                // TODO: normalArgSyntax.IsRef체크
                var appliedParamType = paramType.Apply(outerTypeEnv); // 아직 함수 부분의 TypeEnv가 확정되지 않았으므로, outer까지만 적용하고 나머지는 funcInfo의 TypeVar로 채워넣는다                

                // 현재 파라미터의 타입을 힌트로 준다
                var expResult = ExpIR0ExpTranslator.Translate(normalArgSyntax.Exp, context, appliedParamType, bDerefIfTypeIsRef: true);
                if (!expResult.IsValid(out var exp)) // 에러 발생했으면
                    return TranslationResult.Error<R.Exp?>();

                var castExpResult = BodyMisc.CastExp_Exp(exp, appliedParamType, normalArgSyntax.Exp, context);
                if (!castExpResult.IsValid(out var castExp))
                    return TranslationResult.Error<R.Exp?>();

                expandedArgs.Add(castExp);
                nextExpandedArgIndex++;
                return TranslationResult.Valid<R.Exp?>(castExp);
            }
            else if (argSyntax is S.Argument.Params paramsArgSyntax) // params t
            {
                var expResult = ExpIR0ExpTranslator.Translate(paramsArgSyntax.Exp, context, hintType: null, bDerefIfTypeIsRef: true);
                if (!expResult.IsValid(out var exp))
                    return TranslationResult.Error<R.Exp?>();

                var expType = exp.GetExpType();
                if (expType is not TupleType tupleExpType)
                {
                    context.AddFatalError(A0402_Parameter_ParamsArgumentShouldBeTuple, argSyntax);
                    return TranslationResult.Error<R.Exp?>();
                }
                
                int memberVarCount = tupleExpType.GetMemberVarCount();

                for (int i = 0; i < memberVarCount; i++)
                {
                    tupleExpType.GetMemberVar(i);
                }

                // head
                if (0 < memberVarCount)
                {
                    var memberVar = tupleExpType.GetMemberVar(0);
                    var arg = new FuncMatcherArgument.ParamsHead(exp, memberVarCount, memberVar.GetDeclType());
                    args.Add(arg);
                }

                // rest
                for (int i = 1; i < memberVarCount; i++)
                {
                    var memberVar = tupleExpType.GetMemberVar(i);
                    args.Add(new FuncMatcherArgument.ParamsRest(memberVar.GetDeclType()));
                }
            }
            else
            {
                throw new UnreachableException();
            }
        }
        else
        {
            // 끝
            return TranslationResult.Valid<R.Exp?>(null);
        }
    }
}

struct FuncMatcher 
{
    ScopeContext context;    

    ImmutableArray<FuncParameter> funcParams;
    ImmutableArray<IType> partialTypeArgs;
    

    TypeResolver typeResolver;

    public static FuncMatcherResult Match(ScopeContext context, ImmutableArray<FuncParameter> funcParams, ImmutableArray<IType> partialTypeArgs, ImmutableArray<S.Argument> argsSyntax, TypeResolver typeResolver)
    {
        var matcher = new FuncMatcher
        {
            context = context,
            funcParams = funcParams,
            partialTypeArgs = partialTypeArgs,
            argsSyntax = argsSyntax,

            typeResolver = new NullTypeResolver(partialTypeArgs),
            expandedArgs = new List<R.Exp>(),
            curArgsSyntaxIndex = 0,
            curExpandedArgIndex = 0
        };

        return matcher.Match();
    }

    FuncMatcherResult Match()
    {
        // 앞에서부터 하나씩
        for (int curParamIndex = 0; curParamIndex < funcParams.Length; curParamIndex++)
        {
            var funcParam = funcParams[curParamIndex];

            var argResult = GetNextRArgument(ref funcParam);
            if (!argResult.IsValid(out var optArg))
                return TranslationResult.Error<MatchCallableResult?>();     // 에러

            if (optArg == null)
                return TranslationResult.Valid<MatchCallableResult?>(null); // 실패

            MatchArgument(funcParam, optArg, typeResolver);

            rargsBuilder.Add(argument);
        }


        // 전부

        MatchPartialArguments(0, funcParams.Length, 0, expandedArgs.Length, rargsBuilder);

        var resolvedTypeArgs = typeResolver.Resolve();

        // 뭐가 exact인가: typeParam에 해당하는 typeArgs를 다 적어서 Resolve가 안 필요한 경우 (0개도 포함)
        var bExactMatch = funcParams.Length == typeArgs.Length;
        var rargs =   // MakeRArgs();

        return new MatchCallableResult(bMatch: true, bExactMatch, rargs, resolvedTypeArgs);
    }

    abstract record MatchArgumentResult
    {
        public record Match(R.Exp Argument) : MatchArgumentResult;
        public record Fail : MatchArgumentResult;
        public record Fatal : MatchArgumentResult;
    }

    
}
