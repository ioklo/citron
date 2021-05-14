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

            partial struct FuncMatcher
            {
                // params를 확장한 Argument
                abstract record Argument
                {
                    // preanalyze할수도 있고, 여기서 시작할수도 있다
                    public abstract void DoAnalyze(ref StmtAndExpAnalyzer analyzer, ResolveHint hint);
                    public abstract TypeValue GetTypeValue();

                    // 기본 아규먼트 
                    public record Normal(S.Exp Exp) : Argument
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
                    public record ParamsHead(ExpResult.Exp Exp, TypeValue TypeValue) : Argument
                    {
                        public override void DoAnalyze(ref StmtAndExpAnalyzer analyzer, ResolveHint hint)
                        {
                        }

                        public override TypeValue GetTypeValue()
                        {
                            return TypeValue;
                        }
                    }

                    public record ParamsRest(TypeValue TypeValue) : Argument
                    {
                        public override void DoAnalyze(ref StmtAndExpAnalyzer analyzer, ResolveHint hint)
                        {
                        }

                        public override TypeValue GetTypeValue()
                        {
                            return TypeValue;
                        }
                    }

                    public record Ref(S.Exp Exp) : Argument
                    {
                        public override void DoAnalyze(ref StmtAndExpAnalyzer analyzer, ResolveHint hint)
                        {
                            var result = analyzer.AnalyzeExp(Exp, hint);


                        }

                        public override TypeValue GetTypeValue()
                        {

                        }
                    }
                }
            }

            class FuncMatcherFatalException : Exception
            {

            }
            
            [AutoConstructor]
            partial struct FuncMatcher
            {   
                StmtAndExpAnalyzer analyzer;
                TypeEnv outerTypeEnv;
                M.FuncInfo funcInfo;
                ImmutableArray<TypeValue> typeArgs; // partial, decl: F<T, U>(U u), call: F<int>(u), typeArgs: [int]
                ImmutableArray<S.Argument> sargs;
                
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
                                foreach (var elemType in tupleTypeValue.ElemTypes)
                                {
                                    if (head == null)
                                    {
                                        head = new Argument.ParamsHead(expResult, elemType);
                                        args.Add(head);
                                    }
                                    else
                                    {
                                        args.Add(new Argument.ParamsRest(elemType));
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
                
                // Layer 1
                void MatchPartialArguments(
                    ImmutableArray<TypeValue> paramTypes,
                    int paramsBegin, int paramsEnd,
                    ImmutableArray<Argument> args, int argsBegin, int argsEnd,
                    ref TypeResolver resolver) // throws FuncMatcherFatalException
                {
                    Debug.Assert(paramsEnd - paramsBegin == argsEnd - argsBegin);
                    int paramsCount = paramsEnd - paramsBegin;

                    for (int i = 0; i < paramsCount; i++)
                    {
                        var paramType = paramTypes[paramsBegin + i];
                        var arg = args[argsBegin + 1];

                        MatchArgument(paramType, arg);

                        // MatchArgument마다 Constraint추가
                        resolver.AddConstraint(paramType, arg.GetTypeValue());
                    }
                }

                // Layer 1
                void MatchParamsArguments(
                    TypeValue paramType, 
                    ImmutableArray<Argument> args, int argsBegin, int argsEnd, 
                    ref TypeResolver resolver) // throws FuncMatcherFatalException
                {
                    if (paramType is TupleTypeValue tupleParamType)
                    {
                        if (tupleParamType.ElemTypes.Length != argsEnd - argsBegin)
                            throw new FuncMatcherFatalException();

                        MatchPartialArguments(tupleParamType.ElemTypes, 0, tupleParamType.ElemTypes.Length, args, argsBegin, argsEnd, ref resolver);
                    }
                    else // params T <=> (1, 2, "hi")
                    {
                        var elemTypesBuilder = ImmutableArray.CreateBuilder<TypeValue>(argsEnd - argsBegin);

                        for (int i = argsBegin; i < argsEnd; i++)
                        {
                            var arg = args[i];
                            MatchArgument_UnknownParamType(arg);

                            var argType = arg.GetTypeValue();
                            elemTypesBuilder.Add(argType);
                        }

                        var argTupleType = analyzer.globalContext.GetTupleType(elemTypesBuilder.MoveToImmutable());

                        // Constraint 추가
                        resolver.AddConstraint(paramType, argTupleType);
                    }
                }

                // Layer 1
                void SetupResolver(ref TypeResolver resolver)
                {   
                    // decl: Func<T, U>(List<U> u)
                    // call: Func<int>([1, 2, 3])
                    // 일 경우,
                    
                    // constraint .. (List<U>, typeof([1, 2, 3]))

                    // resolve(List<U>, List<int>)
                    // resolve(U, int) |=> (U => int)
                    // fixpoint iteration? 할 필요 있나

                    // 결과만들기 Func<int, query(U)>

                    // Func2<U>(U u) 
                    // Func<T>()
                    // {
                    //     T t;
                    //     Func2(t); 
                    // }
                    // Func<T, U>(T t, U u)

                    // Func2관점에서 수행 (왼쪽과 오른쪽이 도메인이 다르다)
                    // constraint(U, T)
                    // => constraint(typeVar(0), typeVar(0)); 헷갈림
                }

                // Layer 1
                ImmutableArray<TypeValue> ResolveTypeArgs(ref TypeResolver resolver)
                {
                    resolver.Resolve();

                    resolver.GetType();
                }

                ImmutableArray<TypeValue> MakeParamTypes(ImmutableArray<(M.Type Type, M.Name Name)> parameters)
                {
                    var builder = ImmutableArray.CreateBuilder<TypeValue>(parameters.Length);
                    foreach(var param in parameters)
                    {
                        var type = analyzer.globalContext.GetTypeValueByMType(param.Type);
                        builder.Add(type);
                    }
                    return builder.MoveToImmutable();
                }

                ImmutableArray<R.Exp> MakeRArgs()
                {

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

                        // 2. funcInfo에서 params 앞부분과 뒷부분으로 나누어서 인자 체크를 한다
                        if (funcInfo.ParamInfo.VariadicParamIndex != -1)
                        {
                            //                v
                            // param(4) : 0 1 2 3
                            // args(6)  : 0 1 2 3 4 5                    
                            // wargs(2) : 0 1

                            // front(Params,Args) : [0, v) 
                            // backParams         : [v + 1 ~ parameters.Length)
                            // backArgs           : [args.Length - (parameters.Length - v - 1), args.Length)
                            // front.end <= backArgs.begin 이어야만 한다

                            int v = funcInfo.ParamInfo.VariadicParamIndex;
                            int argsEnd = expandedArgs.Length;
                            int paramsEnd = funcInfo.ParamInfo.Parameters.Length;

                            int backArgsBegin = argsEnd - (paramsEnd - v - 1);

                            if (backArgsBegin < v)
                                throw new FuncMatcherFatalException();

                            var parameters = MakeParamTypes(funcInfo.ParamInfo.Parameters);
                            var resolver = new TypeResolver();

                            SetupResolver(ref resolver);

                            // 앞부분
                            MatchPartialArguments(parameters, 0, v, expandedArgs, 0, v, ref resolver);

                            // 중간 params 부분
                            MatchParamsArguments(parameters[v], expandedArgs, v, backArgsBegin, ref resolver);

                            // 뒷부분
                            MatchPartialArguments(parameters, v + 1, paramsEnd, expandedArgs, backArgsBegin, argsEnd, ref resolver);

                            // typeargs 만들기
                            var resolvedTypeArgs = ResolveTypeArgs(ref resolver);
                            var bExactMatch = funcInfo.ParamInfo.Parameters.Length == typeArgs.Length;
                            var rargs = MakeRArgs();

                            return new MatchArgsResult(true, bExactMatch, rargs, resolvedTypeArgs);
                        }
                        else
                        {
                            // 길이가 다르면 에러
                            if (funcInfo.ParamInfo.Parameters.Length != expandedArgs.Length)
                                throw new FuncMatcherFatalException();

                            var resolver = new TypeResolver();
                            SetupResolver(ref resolver);

                            var parameters = MakeParamTypes(funcInfo.ParamInfo.Parameters);

                            // 전부
                            MatchPartialArguments(parameters, 0, funcInfo.ParamInfo.Parameters.Length, expandedArgs, 0, expandedArgs.Length, ref resolver);

                            var resolvedTypeArgs = ResolveTypeArgs(ref resolver);
                            // 뭐가 exact인가: typeParam에 해당하는 typeArgs를 다 적어서 Resolve가 안 필요한 경우 (0개도 포함)
                            var bExactMatch = funcInfo.ParamInfo.Parameters.Length == typeArgs.Length;
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
