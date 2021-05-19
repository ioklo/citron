﻿using Gum.Collections;
using Pretune;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using S = Gum.Syntax;
using M = Gum.CompileTime;
using R = Gum.IR0;
using Gum.Infra;

namespace Gum.IR0Translator
{
    // 어떤 Exp에서 타입 정보 등을 알아냅니다
    partial class Analyzer
    {
        partial struct StmtAndExpAnalyzer
        {
            [AutoConstructor]
            partial struct MatchArgsResult
            {
                public static readonly MatchArgsResult Invalid = new MatchArgsResult();

                public bool bMatch { get; }
                public bool bExactMatch { get; } // TypeInference를 사용하지 않은 경우                
                public ImmutableArray<R.Argument> Args { get; }
                public ImmutableArray<TypeValue> TypeArgs { get; }
            }

            // params를 확장한 Argument
            abstract class FuncMatcherArgument
            {
                // preanalyze할수도 있고, 여기서 시작할수도 있다
                public abstract void DoAnalyze(ResolveHint hint);
                public abstract TypeValue GetTypeValue();
                public abstract R.Argument? GetRArgument();

                // 기본 아규먼트 
                public class Normal : FuncMatcherArgument
                {
                    StmtAndExpAnalyzer analyzer;
                    S.Exp exp;
                    ExpResult.Exp? expResult;

                    public Normal(StmtAndExpAnalyzer analyzer, S.Exp exp) 
                    { 
                        this.analyzer = analyzer; 
                        this.exp = exp; 
                    }

                    public override void DoAnalyze(ResolveHint hint)
                    {
                        expResult = analyzer.AnalyzeExp_Exp(exp, hint);
                    }                    

                    public override TypeValue GetTypeValue()
                    {
                        Debug.Assert(expResult != null);
                        return expResult.TypeValue;
                    }

                    public override R.Argument? GetRArgument()
                    {
                        Debug.Assert(expResult != null);
                        return new R.Argument.Normal(expResult.Result);
                    }
                }

                // Params가 확장된 아규먼트
                public class ParamsHead : FuncMatcherArgument
                {
                    ExpResult.Exp Exp;
                    int ParamCount;
                    TypeValue TypeValue;

                    public override void DoAnalyze(ResolveHint hint)
                    {
                    }

                    public override TypeValue GetTypeValue()
                    {
                        return TypeValue;
                    }

                    public override R.Argument? GetRArgument()
                    {
                        return new R.Argument.Params(Exp.Result, ParamCount);
                    }
                }

                public record ParamsRest(TypeValue TypeValue) : FuncMatcherArgument
                {
                    public override void DoAnalyze(ResolveHint hint)
                    {
                    }

                    public override TypeValue GetTypeValue()
                    {
                        return TypeValue;
                    }

                    public override R.Argument? GetRArgument()
                    {
                        return null;
                    }
                }

                public record Ref(S.Exp Exp) : FuncMatcherArgument
                {
                    public override void DoAnalyze(ResolveHint hint)
                    {
                        throw new NotImplementedException();
                    }

                    public override TypeValue GetTypeValue()
                    {
                        throw new NotImplementedException();
                    }

                    public override R.Argument? GetRArgument()
                    {
                        throw new NotImplementedException();
                    }
                }
            }

            class FuncMatcherFatalException : Exception
            {
            }

            struct FuncMatcherLayer1
            {
                StmtAndExpAnalyzer analyzer;
                ImmutableArray<TypeValue> paramTypes;
                ImmutableArray<FuncMatcherArgument> args;
                TypeResolver typeResolver;

                public FuncMatcherLayer1(StmtAndExpAnalyzer analyzer, ImmutableArray<TypeValue> paramTypes, ImmutableArray<FuncMatcherArgument> args, TypeResolver typeResolver)
                {
                    this.paramTypes = paramTypes;
                    this.args = args;
                    this.typeResolver = typeResolver;
                }

                
            }

            // Layer 0
            ImmutableArray<FuncMatcherArgument> ExpandArguments(ref StmtAndExpAnalyzer analyzer, ImmutableArray<S.Argument> sargs) // throws FuncMatcherFatalException
            {
                var args = ImmutableArray.CreateBuilder<FuncMatcherArgument>();
                foreach (var sarg in sargs)
                {
                    if (sarg is S.Argument.Normal sNormalArg)
                        args.Add(new FuncMatcherArgument.Normal(sNormalArg.Exp));
                    else if (sarg is S.Argument.Params sParamsArg)
                    {
                        // expanded argument는 먼저 타입을 알아내야 한다
                        ExpResult.Exp expResult;
                        try
                        {
                            expResult = analyzer.AnalyzeExp_Exp(sParamsArg.Exp, ResolveHint.None);
                        }
                        catch (AnalyzerFatalException)
                        {
                            throw new FuncMatcherFatalException();
                        }

                        if (expResult.TypeValue is TupleTypeValue tupleTypeValue)
                        {
                            FuncMatcherArgument.ParamsHead? head = null;
                            int index = 0;
                            foreach (var tupleElem in tupleTypeValue.Elems)
                            {
                                if (head == null)
                                {
                                    head = new FuncMatcherArgument.ParamsHead(expResult, tupleTypeValue.Elems.Length, tupleElem.Type);
                                    args.Add(head);
                                }
                                else
                                {
                                    args.Add(new FuncMatcherArgument.ParamsRest(tupleElem.Type));
                                }

                                index++;
                            }
                        }
                        else
                        {
                            throw new FuncMatcherFatalException();
                        }
                    }
                    else if (sarg is S.Argument.Ref sRefArg)
                    {
                        args.Add(new FuncMatcherArgument.Ref(sRefArg.Exp));
                    }
                }

                return args.ToImmutable();
            }

            // Layer 0
            MatchArgsResult MatchFunc(
                ref StmtAndExpAnalyzer analyzer, 
                TypeEnv outerTypeEnv, 
                M.ParamInfo paramInfo, 
                ImmutableArray<TypeValue> typeArgs, 
                ImmutableArray<S.Argument> sargs) // nothrow
            {
                try
                {
                    var paramTypes = MakeParamTypes(paramInfo.Parameters);
                    var variadicParamIndex = paramInfo.VariadicParamIndex;

                    // 1. 아규먼트에서 params를 확장시킨다 (params에는 타입힌트를 적용하지 않고 먼저 평가한다)
                    var expandedArgs = ExpandArguments(ref analyzer, sargs);
                    var typeResolver = new NullTypeResolver(typeArgs);
                    var matchFuncCore = new MatchFuncCore(paramTypes, typeArgs, expandedArgs, typeResolver);

                    // 2. funcInfo에서 params 앞부분과 뒷부분으로 나누어서 인자 체크를 한다
                    if (variadicParamIndex != null)
                        return matchFuncCore.MatchFuncWithParams(variadicParamIndex);
                    else
                        return matchFuncCore.MatchFuncWithoutParams();
                }
                catch (FuncMatcherFatalException)
                {
                    return MatchArgsResult.Invalid;
                }
            }

            [AutoConstructor]
            partial struct MatchFuncCore
            {
                ImmutableArray<TypeValue> paramTypes;
                ImmutableArray<TypeValue> typeArgs;
                ImmutableArray<FuncMatcherArgument> expandedArgs;
                TypeResolver typeResolver;

                public MatchArgsResult MatchFuncWithoutParams()
                {
                    // 길이가 다르면 에러
                    if (paramTypes.Length != expandedArgs.Length)
                        throw new FuncMatcherFatalException();

                    // 전부
                    MatchPartialArguments(0, paramTypes.Length, 0, expandedArgs.Length);

                    var resolvedTypeArgs = typeResolver.Resolve();
                    
                    // 뭐가 exact인가: typeParam에 해당하는 typeArgs를 다 적어서 Resolve가 안 필요한 경우 (0개도 포함)
                    var bExactMatch = paramTypes.Length == typeArgs.Length;
                    var rargs = MakeRArgs();

                    return new MatchArgsResult(bMatch: true, bExactMatch, rargs, resolvedTypeArgs);
                }

                public MatchArgsResult MatchFuncWithParams(int? variadicParamIndex)
                {
                    //                v
                    // param(4) : 0 1 2 3
                    // args(6)  : 0 1 2 3 4 5                    
                    // wargs(2) : 0 1

                    // front(Params,Args) : [0, v) 
                    // backParams         : [v + 1 ~ parameters.Length)
                    // backArgs           : [args.Length - (parameters.Length - v - 1), args.Length)
                    // front.end <= backArgs.begin 이어야만 한다

                    int v = variadicParamIndex.Value;
                    int argsEnd = expandedArgs.Length;
                    int paramsEnd = paramTypes.Length;

                    int backArgsBegin = argsEnd - (paramsEnd - v - 1);

                    if (backArgsBegin < v)
                        throw new FuncMatcherFatalException();

                    // TODO: TypeResolver 완성
                    var resolver = new NullTypeResolver(typeArgs);

                    // 앞부분
                    MatchPartialArguments(0, v, 0, v);

                    // 중간 params 부분
                    MatchParamsArguments(v, v, backArgsBegin);

                    // 뒷부분
                    MatchPartialArguments(v + 1, paramsEnd, backArgsBegin, argsEnd);

                    // typeargs 만들기
                    var resolvedTypeArgs = typeResolver.Resolve();
                    var bExactMatch = paramTypes.Length == typeArgs.Length;
                    var rargs = MakeRArgs();

                    return new MatchArgsResult(true, bExactMatch, rargs, resolvedTypeArgs);
                }

                #region Layer1                
                void MatchPartialArguments(int paramsBegin, int paramsEnd, int argsBegin, int argsEnd) // throws FuncMatcherFatalException
                {
                    Debug.Assert(paramsEnd - paramsBegin == argsEnd - argsBegin);
                    int paramsCount = paramsEnd - paramsBegin;

                    for (int i = 0; i < paramsCount; i++)
                    {
                        var paramType = paramTypes[paramsBegin + i];
                        var arg = args[argsBegin + i];

                        MatchArgument(paramType, arg);

                        // MatchArgument마다 Constraint추가
                        typeResolver.AddConstraint(paramType, arg.GetTypeValue());
                    }
                }

                void MatchParamsArguments(int paramIndex, int argsBegin, int argsEnd) // throws FuncMatcherFatalException
                {
                    var paramType = paramTypes[paramIndex];

                    if (paramType is TupleTypeValue tupleParamType)
                    {
                        if (tupleParamType.Elems.Length != argsEnd - argsBegin)
                            throw new FuncMatcherFatalException();

                        int paramsCount = tupleParamType.Elems.Length;
                        Debug.Assert(paramsCount == argsEnd - argsBegin);

                        for (int i = 0; i < paramsCount; i++)
                        {
                            var tupleElemType = tupleParamType.Elems[i].Type;
                            var arg = args[argsBegin + i];

                            MatchArgument(tupleElemType, arg);

                            // MatchArgument마다 Constraint추가
                            typeResolver.AddConstraint(tupleElemType, arg.GetTypeValue());
                        }
                    }
                    else // params T <=> (1, 2, "hi")
                    {
                        var argsLength = argsEnd - argsBegin;
                        var elemBuilder = ImmutableArray.CreateBuilder<(TypeValue Type, string? Name)>(argsLength);

                        for (int i = 0; i < argsLength; i++)
                        {
                            var arg = args[i];
                            MatchArgument_UnknownParamType(arg);

                            var argType = arg.GetTypeValue();
                            elemBuilder.Add((argType, null)); // unnamed tuple element
                        }

                        var argTupleType = analyzer.globalContext.GetTupleType(elemBuilder.MoveToImmutable());

                        // Constraint 추가
                        typeResolver.AddConstraint(paramType, argTupleType);
                    }
                }


                // Layer 1
                ImmutableArray<R.Argument> MakeRArgs()
                {
                    var builder = ImmutableArray.CreateBuilder<R.Argument>();

                    foreach(var arg in expandedArgs)
                    {
                        var rarg = arg.GetRArgument();
                        if (rarg == null) continue;

                        builder.Add(rarg);
                    }

                    return builder.ToImmutable();
                }

                #endregion

                #region Layer 2

                // Layer 2
                void MatchArgument(TypeValue paramType, FuncMatcherArgument arg) // throws FuncMatcherFatalException
                {
                    var appliedParamType = paramType.Apply_TypeValue(outerTypeEnv); // 아직 함수 부분의 TypeEnv가 확정되지 않았으므로, outer까지만 적용하고 나머지는 funcInfo의 TypeVar로 채워넣는다

                    // arg
                    var resolveHint = ResolveHint.Make(appliedParamType);
                    arg.DoAnalyze(ref analyzer, resolveHint); // Argument 종류에 따라서 달라진다

                    var argType = arg.GetTypeValue();

                    if (!analyzer.globalContext.IsAssignable(appliedParamType, argType))
                        throw new FuncMatcherFatalException();
                }

                // Layer 2
                void MatchArgument_UnknownParamType(FuncMatcherArgument arg)
                {
                    arg.DoAnalyze(ref analyzer, ResolveHint.None);
                }
                #endregion
            }
        }
    }
}
