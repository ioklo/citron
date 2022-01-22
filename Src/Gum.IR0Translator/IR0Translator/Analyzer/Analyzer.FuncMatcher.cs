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
using Gum.Infra;
using static Gum.IR0Translator.AnalyzeErrorCode;
using Gum.Analysis;

namespace Gum.IR0Translator
{
    // 어떤 Exp에서 타입 정보 등을 알아냅니다
    partial class Analyzer
    {
        // entry1의 result        
        abstract record FuncMatchIndexResult
        {
            public record MultipleCandidates : FuncMatchIndexResult
            {
                public static readonly MultipleCandidates Instance = new MultipleCandidates();
                private MultipleCandidates() { }
            }

            public record NotFound : FuncMatchIndexResult
            {
                public static readonly NotFound Instance = new NotFound();
                private NotFound() { }
            }
            
            public record Success(int Index, ImmutableArray<ITypeSymbol> TypeArgs, ImmutableArray<R.Argument> Args) : FuncMatchIndexResult;
        }

        // entry2의 result
        [AutoConstructor]
        partial struct FuncMatchResult
        {
            public ImmutableArray<ITypeSymbol> TypeArgs { get; }
            public ImmutableArray<R.Argument> Args { get; }
        }

        partial struct FuncMatcher
        {
            [AutoConstructor]
            partial struct MatchedFunc
            {
                public MatchCallableResult Result { get; }
                public int Index { get; }
                public GlobalContext GlobalContext { get; }
                public ICallableContext CallableContext { get; }
                public LocalContext LocalContext { get; }
            }
            
            [AutoConstructor]
            partial struct MatchCallableResult
            {
                public static readonly MatchCallableResult Invalid = new MatchCallableResult();

                public bool bMatch { get; }
                public bool bExactMatch { get; } // TypeInference를 사용하지 않은 경우                
                public ImmutableArray<R.Argument> Args { get; }
                public ImmutableArray<ITypeSymbol> TypeArgs { get; }
            }

            class FuncMatcherFatalException : Exception
            {
            }

            ImmutableArray<FuncParameter> paramInfos;
            ImmutableArray<ITypeSymbol> typeArgs;
            ImmutableArray<FuncMatcherArgument> expandedArgs;
            TypeResolver typeResolver;

            GlobalContext globalContext;
            TypeEnv outerTypeEnv;

            public interface IHaveParameters
            {
                int GetParameterCount();
                FuncParameter GetParameter(int index);
            }            

            // entry 1
            public static FuncMatchIndexResult MatchIndex<TFuncDeclSymbol>(
                GlobalContext globalContext,
                ICallableContext callableContext,
                LocalContext localContext,
                TypeEnv outerTypeEnv, 
                ImmutableArray<TFuncDeclSymbol> funcDecls,
                ImmutableArray<S.Argument> sargs, 
                ImmutableArray<ITypeSymbol> typeArgs)
                where TFuncDeclSymbol : IFuncDeclSymbol
            {
                // 여러 함수 중에서 인자가 맞는것을 선택해야 한다
                var exactCandidates = new Candidates<MatchedFunc?>();
                var restCandidates = new Candidates<MatchedFunc?>();

                // Type inference
                for(int i = 0; i < funcDecls.Length; i++)
                {
                    var funcDecl = funcDecls[i];

                    var cloneContext = CloneContext.Make();
                    var clonedGlobalContext = cloneContext.GetClone(globalContext);
                    var clonedCallableContext = cloneContext.GetClone(callableContext);
                    var clonedLocalContext = cloneContext.GetClone(localContext);

                    var paramTypes = ImmutableArray.CreateRange(funcDecl.GetParameterCount, index => funcDecl.GetParameter(index));
                    var variadicParamIndex = GetVariadicParamIndex(funcDecl);

                    var matchResult = MatchCallableCore(clonedGlobalContext, clonedCallableContext, clonedLocalContext, outerTypeEnv, paramTypes, variadicParamIndex, typeArgs, sargs);

                    if (!matchResult.bMatch) continue;

                    var matchedCandidate = new MatchedFunc(matchResult, i, clonedGlobalContext, clonedCallableContext, clonedLocalContext);

                    if (matchResult.bExactMatch)
                        exactCandidates.Add(matchedCandidate); // context만 저장하게 수정
                    else
                        restCandidates.Add(matchedCandidate);
                }

                // 매칭 된 것으로
                MatchedFunc matchedFunc;
                var exactMatch = exactCandidates.GetSingle();
                if (exactMatch != null)
                {
                    matchedFunc = exactMatch.Value;
                }
                else if (exactCandidates.HasMultiple)
                {
                    return FuncMatchIndexResult.MultipleCandidates.Instance;
                }
                else // empty
                {
                    Debug.Assert(exactCandidates.IsEmpty);

                    var restMatch = restCandidates.GetSingle();
                    if (restMatch != null)
                    {
                        matchedFunc = restMatch.Value;
                    }
                    else if (restCandidates.HasMultiple)
                    {
                        return FuncMatchIndexResult.MultipleCandidates.Instance;
                    }
                    else // empty
                    {
                        return FuncMatchIndexResult.NotFound.Instance;
                    }
                }

                // 확정된 context로 업데이트
                var updateContext = UpdateContext.Make();
                updateContext.Update(globalContext, matchedFunc.GlobalContext);
                updateContext.Update(callableContext, matchedFunc.CallableContext);
                updateContext.Update(localContext, matchedFunc.LocalContext);

                return new FuncMatchIndexResult.Success(matchedFunc.Index, matchedFunc.Result.TypeArgs, matchedFunc.Result.Args);
            }

            // entry 2
            public static FuncMatchResult? Match(
                GlobalContext globalContext,
                ICallableContext callableContext,
                LocalContext localContext,
                TypeEnv outerTypeEnv,
                ImmutableArray<FuncParameter> paramTypes,
                int? variadicParamIndex,
                ImmutableArray<ITypeSymbol> typeArgs,
                ImmutableArray<S.Argument> sargs)
            {
                var result = MatchCallableCore(globalContext, callableContext, localContext, outerTypeEnv, paramTypes, variadicParamIndex, typeArgs, sargs);
                if (result.bMatch)
                    return new FuncMatchResult(result.TypeArgs, result.Args);
                else
                    return null;
            }

            FuncMatcher(ImmutableArray<FuncParameter> paramInfos, ImmutableArray<ITypeSymbol> typeArgs, ImmutableArray<FuncMatcherArgument> expandedArgs, TypeResolver typeResolver, GlobalContext globalContext, TypeEnv outerTypeEnv)
            {
                this.paramInfos = paramInfos;
                this.typeArgs = typeArgs;
                this.expandedArgs = expandedArgs;
                this.typeResolver = typeResolver;
                this.globalContext = globalContext;
                this.outerTypeEnv = outerTypeEnv;
            }
            
            // Match에서만 쓰인다
            static int? GetVariadicParamIndex(IFuncDeclSymbol funcDecl)
            {
                int? variadicParamsIndex = null;
                int paramCount = funcDecl.GetParameterCount();

                for(int i = 0; i < paramCount; i++)
                {
                    var param = funcDecl.GetParameter(i);

                    if (param.Kind == FuncParameterKind.Params)
                    {
                        Debug.Assert(variadicParamsIndex == null);
                        variadicParamsIndex = i;
                    }
                }

                return variadicParamsIndex;
            }

            static MatchCallableResult MatchCallableCore(
                GlobalContext globalContext,
                ICallableContext callableContext,
                LocalContext localContext,
                TypeEnv outerTypeEnv,
                ImmutableArray<FuncParameter> paramTypes,
                int? variadicParamIndex,
                ImmutableArray<ITypeSymbol> typeArgs,
                ImmutableArray<S.Argument> sargs) // nothrow
            {
                try
                {
                    // 1. 아규먼트에서 params를 확장시킨다 (params에는 타입힌트를 적용하지 않고 먼저 평가한다)
                    var expandedArgs = ExpandArguments(globalContext, callableContext, localContext, sargs);
                    var typeResolver = new NullTypeResolver(typeArgs);

                    var inst = new FuncMatcher(paramTypes, typeArgs, expandedArgs, typeResolver, globalContext, outerTypeEnv);

                    // 2. funcInfo에서 params 앞부분과 뒷부분으로 나누어서 인자 체크를 한다
                    if (variadicParamIndex != null)
                        return inst.MatchFuncWithParams(variadicParamIndex.Value);
                    else
                        return inst.MatchFuncWithoutParams();
                }
                catch (FuncMatcherFatalException)
                {
                    return MatchCallableResult.Invalid;
                }
            }

            // params를 확장한 Argument
            abstract class FuncMatcherArgument
            {
                // preanalyze할수도 있고, 여기서 시작할수도 있다
                public abstract void DoAnalyze(FuncParameter? expectType); // throws FuncMatchFatalException
                public abstract ITypeSymbol GetTypeValue();
                public abstract R.Argument? GetRArgument();

                // 기본 아규먼트 
                public class Normal : FuncMatcherArgument
                {
                    StmtAndExpAnalyzer analyzer;
                    GlobalContext globalContext;
                    S.Exp sexp;
                    R.Exp? exp;

                    public Normal(GlobalContext globalContext, ICallableContext callableContext, LocalContext localContext, S.Exp sexp)
                    {
                        this.analyzer = new StmtAndExpAnalyzer(globalContext, callableContext, localContext);
                        this.globalContext = globalContext;
                        this.sexp = sexp;
                        this.exp = null;
                    }

                    public override void DoAnalyze(FuncParameter? paramInfo) // throws FuncMatcherFatalException
                    {
                        if (paramInfo != null)
                        {
                            // ref 파라미터를 원했는데, ref가 안달려 나온 경우, box타입이면 가능하다
                            if (paramInfo.Value.Kind == FuncParameterKind.Ref)
                            {
                                // void F(ref int i) {...}
                                // F(box 3); // 가능
                                throw new NotImplementedException();
                            }
                            else
                            {
                                var hint = ResolveHint.Make(paramInfo.Value.Type);
                                var argResult = analyzer.AnalyzeExp_Exp(sexp, hint);
                                exp = globalContext.TryCastExp_Exp(argResult, paramInfo.Value.Type);

                                if (exp == null)
                                    throw new FuncMatcherFatalException();
                            }
                        }
                        else // none, EnumConstructor
                        {
                            exp = analyzer.AnalyzeExp_Exp(sexp, ResolveHint.None);
                        }
                    }

                    public override ITypeSymbol GetTypeValue()
                    {
                        Debug.Assert(exp != null);
                        return exp.GetTypeSymbol();
                    }

                    public override R.Argument? GetRArgument()
                    {
                        Debug.Assert(exp != null);
                        return new R.Argument.Normal(exp);
                    }
                }

                // Params가 확장된 아규먼트
                public class ParamsHead : FuncMatcherArgument
                {
                    R.Exp exp;
                    int paramCount;
                    ITypeSymbol type;

                    public ParamsHead(R.Exp exp, int paramCount, ITypeSymbol type)
                    {
                        this.exp = exp;
                        this.paramCount = paramCount;
                        this.type = type;
                    }

                    public override void DoAnalyze(FuncParameter? paramInfo) // throws FuncMatcherFatalException
                    {
                        if (paramInfo != null)
                        {
                            // exact match
                            if (!paramInfo.Value.Type.Equals(type))
                                throw new FuncMatcherFatalException();
                        }
                    }

                    public override ITypeSymbol GetTypeValue()
                    {
                        return type;
                    }

                    public override R.Argument? GetRArgument()
                    {
                        return new R.Argument.Params(exp, paramCount);
                    }
                }

                public class ParamsRest : FuncMatcherArgument
                {
                    ITypeSymbol type;

                    public ParamsRest(ITypeSymbol type)
                    {
                        this.type = type;
                    }

                    public override void DoAnalyze(FuncParameter? paramInfo)
                    {
                        if (paramInfo != null)
                        {
                            // exact match
                            if (!paramInfo.Value.Type.Equals(type))
                                throw new FuncMatcherFatalException();
                        }
                    }

                    public override ITypeSymbol GetTypeValue()
                    {
                        return type;
                    }

                    public override R.Argument? GetRArgument()
                    {
                        return null;
                    }
                }

                public class Ref : FuncMatcherArgument
                {
                    StmtAndExpAnalyzer analyzer;
                    S.Exp exp;
                    ExpResult.Loc? locResult;

                    public Ref(GlobalContext globalContext, ICallableContext callableContext, LocalContext localContext, S.Exp exp)
                    {
                        this.analyzer = new StmtAndExpAnalyzer(globalContext, callableContext, localContext);
                        this.exp = exp;
                    }

                    public override void DoAnalyze(FuncParameter? paramInfo) // throws FuncMatcherFatalException
                    {
                        if (paramInfo != null)
                        {
                            // 1. void F(int i) { ... } 파라미터에 ref가 안 붙은 경우, 매칭을 하지 않는다
                            // F(ref j);
                            if (paramInfo.Value.Kind != FuncParameterKind.Ref)
                                throw new FuncMatcherFatalException();

                            // 2. void F(ref int i)
                            var argResult = analyzer.AnalyzeExp(exp, ResolveHint.None);

                            switch (argResult)
                            {
                                case ExpResult.Loc locResult:
                                    this.locResult = locResult;
                                    break;

                                default:
                                    throw new FuncMatcherFatalException();
                            }
                        }
                    }

                    public override ITypeSymbol GetTypeValue()
                    {
                        Debug.Assert(locResult != null);
                        return locResult.TypeSymbol;
                    }

                    public override R.Argument? GetRArgument()
                    {
                        Debug.Assert(locResult != null);
                        return new R.Argument.Ref(locResult.Result);
                    }
                }
            }

            static ImmutableArray<FuncMatcherArgument> ExpandArguments(GlobalContext globalContext, ICallableContext callableContext, LocalContext localContext, ImmutableArray<S.Argument> sargs) // throws FuncMatcherFatalException
            {
                var args = ImmutableArray.CreateBuilder<FuncMatcherArgument>();
                foreach (var sarg in sargs)
                {
                    if (sarg is S.Argument.Normal sNormalArg)
                    {
                        args.Add(new FuncMatcherArgument.Normal(globalContext, callableContext, localContext, sNormalArg.Exp));
                    }
                    else if (sarg is S.Argument.Params sParamsArg)
                    {
                        // expanded argument는 먼저 타입을 알아내야 한다
                        R.Exp exp;
                        try
                        {
                            exp = StmtAndExpAnalyzer.AnalyzeExp_Exp(globalContext, callableContext, localContext, sParamsArg.Exp, ResolveHint.None);
                        }
                        catch (AnalyzerFatalException)
                        {
                            throw new FuncMatcherFatalException();
                        }

                        var expType = exp.GetType();

                        if (expType is TupleTypeSymbol tupleExpType)
                        {
                            FuncMatcherArgument.ParamsHead? head = null;
                            int index = 0;
                            foreach (var tupleElem in tupleExpType.Elems)
                            {
                                if (head == null)
                                {
                                    head = new FuncMatcherArgument.ParamsHead(exp, tupleExpType.Elems.Length, tupleElem.Type);
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
                        args.Add(new FuncMatcherArgument.Ref(globalContext, callableContext, localContext, sRefArg.Exp));
                    }
                }

                return args.ToImmutable();
            }

            MatchCallableResult MatchFuncWithoutParams()
            {
                // 길이가 다르면 에러
                if (paramInfos.Length != expandedArgs.Length)
                    throw new FuncMatcherFatalException();

                // 전부
                MatchPartialArguments(0, paramInfos.Length, 0, expandedArgs.Length);

                var resolvedTypeArgs = typeResolver.Resolve();

                // 뭐가 exact인가: typeParam에 해당하는 typeArgs를 다 적어서 Resolve가 안 필요한 경우 (0개도 포함)
                var bExactMatch = paramInfos.Length == typeArgs.Length;
                var rargs = MakeRArgs();

                return new MatchCallableResult(bMatch: true, bExactMatch, rargs, resolvedTypeArgs);
            }

            MatchCallableResult MatchFuncWithParams(int variadicParamIndex)
            {
                //                v
                // param(4) : 0 1 2 3
                // args(6)  : 0 1 2 3 4 5                    
                // wargs(2) : 0 1

                // front(Params,Args) : [0, v) 
                // backParams         : [v + 1 ~ parameters.Length)
                // backArgs           : [args.Length - (parameters.Length - v - 1), args.Length)
                // front.end <= backArgs.begin 이어야만 한다

                int v = variadicParamIndex;
                int argsEnd = expandedArgs.Length;
                int paramsEnd = paramInfos.Length;

                int backArgsBegin = argsEnd - (paramsEnd - v - 1);

                if (backArgsBegin < v)
                    throw new FuncMatcherFatalException();

                // 앞부분
                MatchPartialArguments(0, v, 0, v);

                // 중간 params 부분
                MatchParamsArguments(v, v, backArgsBegin);

                // 뒷부분
                MatchPartialArguments(v + 1, paramsEnd, backArgsBegin, argsEnd);

                // typeargs 만들기
                var resolvedTypeArgs = typeResolver.Resolve();
                var bExactMatch = paramInfos.Length == typeArgs.Length;
                var rargs = MakeRArgs();

                return new MatchCallableResult(true, bExactMatch, rargs, resolvedTypeArgs);
            }

            #region Layer1                
            void MatchPartialArguments(int paramsBegin, int paramsEnd, int argsBegin, int argsEnd) // throws FuncMatcherFatalException
            {
                Debug.Assert(paramsEnd - paramsBegin == argsEnd - argsBegin);
                int paramsCount = paramsEnd - paramsBegin;

                for (int i = 0; i < paramsCount; i++)
                {
                    var paramType = paramInfos[paramsBegin + i];
                    var arg = expandedArgs[argsBegin + i];

                    MatchArgument(paramType, arg);

                    // MatchArgument마다 Constraint추가
                    typeResolver.AddConstraint(paramType.Type, arg.GetTypeValue());
                }
            }

            void MatchParamsArguments(int paramIndex, int argsBegin, int argsEnd) // throws FuncMatcherFatalException
            {
                var paramInfo = paramInfos[paramIndex];
                Debug.Assert(paramInfo.Kind == FuncParameterKind.Params);

                if (paramInfo.Type is TupleTypeSymbol tupleParamType)
                {
                    if (tupleParamType.Elems.Length != argsEnd - argsBegin)
                        throw new FuncMatcherFatalException();

                    int paramsCount = tupleParamType.Elems.Length;
                    Debug.Assert(paramsCount == argsEnd - argsBegin);

                    for (int i = 0; i < paramsCount; i++)
                    {
                        var tupleElemType = tupleParamType.Elems[i].Type;
                        var arg = expandedArgs[argsBegin + i];

                        MatchArgument(new FuncParameter(FuncParameterKind.Default, tupleElemType, tupleParamType.Elems[i].Name), arg);

                        // MatchArgument마다 Constraint추가
                        typeResolver.AddConstraint(tupleElemType, arg.GetTypeValue());
                    }
                }
                else // params T <=> (1, 2, "hi")
                {
                    var argsLength = argsEnd - argsBegin;
                    var elemBuilder = ImmutableArray.CreateBuilder<(ITypeSymbol Type, string? Name)>(argsLength);

                    for (int i = 0; i < argsLength; i++)
                    {
                        var arg = expandedArgs[i];
                        MatchArgument_UnknownParamType(arg);

                        var argType = arg.GetTypeValue();
                        elemBuilder.Add((argType, null)); // unnamed tuple element
                    }

                    var argTupleType = globalContext.GetTupleType(elemBuilder.MoveToImmutable());

                    // Constraint 추가
                    typeResolver.AddConstraint(paramInfo.Type, argTupleType);
                }
            }

            // Layer 1
            ImmutableArray<R.Argument> MakeRArgs()
            {
                var builder = ImmutableArray.CreateBuilder<R.Argument>();

                foreach (var arg in expandedArgs)
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
            void MatchArgument(FuncParameter param, FuncMatcherArgument arg) // throws FuncMatcherFatalException
            {
                var appliedParam = param.Apply(outerTypeEnv); // 아직 함수 부분의 TypeEnv가 확정되지 않았으므로, outer까지만 적용하고 나머지는 funcInfo의 TypeVar로 채워넣는다
                arg.DoAnalyze(appliedParam); // Argument 종류에 따라서 달라진다
            }

            // Layer 2
            void MatchArgument_UnknownParamType(FuncMatcherArgument arg)
            {
                arg.DoAnalyze(null);
            }
            #endregion
        }

        // 참고자료
        //StmtAndExpAnalyzer CloneAnalyzer()
        //{
        //    var cloneContext = CloneContext.Make();
        //    var clonedGlobalContext = cloneContext.GetClone(globalContext);
        //    var clonedCallableContext = cloneContext.GetClone(callableContext);
        //    var clonedLocalContext = cloneContext.GetClone(localContext);

        //    return new StmtAndExpAnalyzer(clonedGlobalContext, clonedCallableContext, clonedLocalContext);
        //}

        //void UpdateAnalyzer(GlobalContext srcGlobalContext, ICallableContext srcCallableContext, LocalContext srcLocalContext)
        //{
        //    var updateContext = UpdateContext.Make();

        //    updateContext.Update(globalContext, srcGlobalContext);
        //    updateContext.Update(callableContext, srcCallableContext);
        //    updateContext.Update(localContext, srcLocalContext);
        //}




    }
}
