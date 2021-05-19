using Gum.Collections;
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
            abstract record FuncMatcherArgument
            {
                // preanalyze할수도 있고, 여기서 시작할수도 있다
                public abstract void DoAnalyze(ref StmtAndExpAnalyzer analyzer, ResolveHint hint);
                public abstract TypeValue GetTypeValue();

                // 기본 아규먼트 
                public record Normal(S.Exp Exp) : FuncMatcherArgument
                {
                    public ExpResult.Exp? expResult;

                    public override void DoAnalyze(ref StmtAndExpAnalyzer analyzer, ResolveHint hint)
                    {
                        expResult = analyzer.AnalyzeExp_Exp(Exp, hint);
                    }

                    public override TypeValue GetTypeValue()
                    {
                        Debug.Assert(expResult != null);
                        return expResult.TypeValue;
                    }
                }

                // Params가 확장된 아규먼트
                public record ParamsHead(ExpResult.Exp Exp, TypeValue TypeValue) : FuncMatcherArgument
                {
                    public override void DoAnalyze(ref StmtAndExpAnalyzer analyzer, ResolveHint hint)
                    {
                    }

                    public override TypeValue GetTypeValue()
                    {
                        return TypeValue;
                    }
                }

                public record ParamsRest(TypeValue TypeValue) : FuncMatcherArgument
                {
                    public override void DoAnalyze(ref StmtAndExpAnalyzer analyzer, ResolveHint hint)
                    {
                    }

                    public override TypeValue GetTypeValue()
                    {
                        return TypeValue;
                    }
                }

                public record Ref(S.Exp Exp) : FuncMatcherArgument
                {
                    public override void DoAnalyze(ref StmtAndExpAnalyzer analyzer, ResolveHint hint)
                    {
                        throw new NotImplementedException();
                    }

                    public override TypeValue GetTypeValue()
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
                ImmutableArray<TypeValue> paramTypes;
                ImmutableArray<FuncMatcherArgument> args;
                TypeResolver typeResolver;

                public FuncMatcherLayer1(ImmutableArray<TypeValue> paramTypes, ImmutableArray<Argument> args, TypeResolver typeResolver)
                {
                    this.paramTypes = paramTypes;
                    this.args = args;
                    this.typeResolver = typeResolver;
                }

                // Layer 1
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

                // Layer 1
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
                ImmutableArray<TypeValue> ResolveTypeArgs(TypeResolver resolver)
                {
                    return resolver.Resolve();
                }

                // Layer 1
                ImmutableArray<R.Argument> MakeRArgs()
                {
                    throw new NotImplementedException();
                }
            }

            [AutoConstructor]
            partial struct FuncMatcher
            {   
                StmtAndExpAnalyzer analyzer;
                TypeEnv outerTypeEnv;
                int? variadicParamIndex;
                ImmutableArray<TypeValue> paramTypes;
                ImmutableArray<TypeValue> typeArgs; // partial, decl: F<T, U>(U u), call: F<int>(u), typeArgs: [int]
                ImmutableArray<S.Argument> sargs;
                
                // Layer 0
                ImmutableArray<Argument> ExpandArguments() // throws FuncMatcherFatalException
                {
                    var args = ImmutableArray.CreateBuilder<Argument>();
                    foreach (var sarg in sargs)
                    {
                        if (sarg is S.Argument.Normal sNormalArg)
                            args.Add(new Argument.Normal(sNormalArg.Exp));
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
                                Argument.ParamsHead? head = null;
                                int index = 0;
                                foreach (var tupleElem in tupleTypeValue.Elems)
                                {
                                    if (head == null)
                                    {
                                        head = new Argument.ParamsHead(expResult, tupleElem.Type);
                                        args.Add(head);
                                    }
                                    else
                                    {
                                        args.Add(new Argument.ParamsRest(tupleElem.Type));
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
                            args.Add(new Argument.Ref(sRefArg.Exp));
                        }
                    }

                    return args.ToImmutable();
                }

                // Layer 2
                void MatchArgument(TypeValue paramType, Argument arg) // throws FuncMatcherFatalException
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
                void MatchArgument_UnknownParamType(Argument arg)
                {
                    arg.DoAnalyze(ref analyzer, ResolveHint.None);
                }
                
                // typeEnv는 funcInfo미 포함 타입정보
                // typeArgs가 충분하지 않을 수 있다. 나머지는 inference로 채운다
                // Layer 0
                public MatchArgsResult Match() // nothrow
                {
                    try
                    {
                        // 1. 아규먼트에서 params를 확장시킨다 (params에는 타입힌트를 적용하지 않고 먼저 평가한다)
                        var expandedArgs = ExpandArguments();

                        new FuncMatcherLayer1(paramTypes, expandedArgs, typeResolver);

                        // 2. funcInfo에서 params 앞부분과 뒷부분으로 나누어서 인자 체크를 한다
                        if (variadicParamIndex != null)
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

                            SetupResolver(resolver);

                            // 앞부분
                            MatchPartialArguments(0, v, expandedArgs, 0, v, resolver);

                            // 중간 params 부분
                            MatchParamsArguments(v, expandedArgs, v, backArgsBegin, resolver);

                            // 뒷부분
                            MatchPartialArguments(v + 1, paramsEnd, expandedArgs, backArgsBegin, argsEnd, resolver);

                            // typeargs 만들기
                            var resolvedTypeArgs = ResolveTypeArgs(resolver);
                            var bExactMatch = paramTypes.Length == typeArgs.Length;
                            var rargs = MakeRArgs();

                            return new MatchArgsResult(true, bExactMatch, rargs, resolvedTypeArgs);
                        }
                        else
                        {
                            // 길이가 다르면 에러
                            if (paramTypes.Length != expandedArgs.Length)
                                throw new FuncMatcherFatalException();

                            var resolver = new NullTypeResolver(typeArgs);
                            SetupResolver(resolver);                            

                            // 전부
                            MatchPartialArguments(0, paramTypes.Length, expandedArgs, 0, expandedArgs.Length, resolver);

                            var resolvedTypeArgs = ResolveTypeArgs(resolver);
                            // 뭐가 exact인가: typeParam에 해당하는 typeArgs를 다 적어서 Resolve가 안 필요한 경우 (0개도 포함)
                            var bExactMatch = paramTypes.Length == typeArgs.Length;
                            var rargs = MakeRArgs();

                            return new MatchArgsResult(bMatch: true, bExactMatch, rargs, resolvedTypeArgs);
                        }
                    }
                    catch(FuncMatcherFatalException)
                    {
                        return MatchArgsResult.Invalid;
                    }
                }
            }
        }
    }
}
