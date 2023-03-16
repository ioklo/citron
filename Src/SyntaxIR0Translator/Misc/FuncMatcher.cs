using Citron.Collections;
using Pretune;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using S = Citron.Syntax;
using R = Citron.IR0;
using Citron.Infra;
using static Citron.Analysis.SyntaxAnalysisErrorCode;
using Citron.Symbol;

namespace Citron.Analysis
{
    // 어떤 Exp에서 타입 정보 등을 알아냅니다
    
    // entry1의 result        
    abstract record class FuncMatchIndexResult
    {
        public record class MultipleCandidates : FuncMatchIndexResult
        {
            public static readonly MultipleCandidates Instance = new MultipleCandidates();
            private MultipleCandidates() { }
        }

        public record class NotFound : FuncMatchIndexResult
        {
            public static readonly NotFound Instance = new NotFound();
            private NotFound() { }
        }
            
        public record class Success(int Index, ImmutableArray<IType> TypeArgs, ImmutableArray<R.Argument> Args) : FuncMatchIndexResult;
    }

    // entry2의 result    
    record struct FuncMatchResult(ImmutableArray<IType> TypeArgs, ImmutableArray<R.Argument> Args);

    struct FuncMatcher
    {        
        record struct MatchedFunc(MatchCallableResult Result, int Index, ScopeContext Context);
        // bExactMatch: TypeInference를 사용하지 않은 경우
        record struct MatchCallableResult(bool bMatch, bool bExactMatch, ImmutableArray<R.Argument> Args, ImmutableArray<IType> TypeArgs);
        class FuncMatcherFatalException : Exception { }

        ImmutableArray<FuncParameter> paramInfos;
        ImmutableArray<IType> typeArgs;
        ImmutableArray<FuncMatcherArgument> expandedArgs;
        TypeResolver typeResolver;

        TypeEnv outerTypeEnv;

        public interface IHaveParameters
        {
            int GetParameterCount();
            FuncParameter GetParameter(int index);
        }            

        // entry 1
        public static FuncMatchIndexResult MatchIndex<TFuncDeclSymbol>(
            ScopeContext scopeContext,
            TypeEnv outerTypeEnv, 
            ImmutableArray<TFuncDeclSymbol> funcDecls,
            ImmutableArray<S.Argument> sargs, 
            ImmutableArray<IType> typeArgs)
            where TFuncDeclSymbol : IFuncDeclSymbol
        {
            // 여러 함수 중에서 인자가 맞는것을 선택해야 한다
            var exactCandidates = new Candidates<MatchedFunc>();
            var restCandidates = new Candidates<MatchedFunc>();

            // Type inference
            for(int i = 0; i < funcDecls.Length; i++)
            {
                var funcDecl = funcDecls[i];

                var cloneContext = new CloneContext();
                var clonedScopeContext = cloneContext.GetClone(scopeContext);

                var paramTypes = ImmutableArray.CreateRange(funcDecl.GetParameterCount, index => funcDecl.GetParameter(index));
                var variadicParamIndex = GetVariadicParamIndex(funcDecl);

                var matchResult = MatchCallableCore(clonedScopeContext, outerTypeEnv, paramTypes, variadicParamIndex, typeArgs, sargs);
                if (matchResult == null) continue;

                var matchedCandidate = new MatchedFunc(matchResult.Value, i, clonedScopeContext);

                if (matchResult.Value.bExactMatch)
                    exactCandidates.Add(matchedCandidate); // context만 저장하게 수정
                else
                    restCandidates.Add(matchedCandidate);
            }

            // 매칭 된 것으로
            MatchedFunc matchedFunc;
            var exactMatchResult = exactCandidates.GetUniqueResult();
            if (!exactMatchResult.IsFound(out matchedFunc))
            {
                if (exactMatchResult.IsMultipleError())
                {
                    return FuncMatchIndexResult.MultipleCandidates.Instance;
                }
                else if (exactMatchResult.IsNotFound()) // empty
                {
                    var restMatchResult = restCandidates.GetUniqueResult();
                    if (!restMatchResult.IsFound(out matchedFunc))
                    {
                        if (restMatchResult.IsMultipleError()) return FuncMatchIndexResult.MultipleCandidates.Instance;
                        else if (restMatchResult.IsNotFound()) return FuncMatchIndexResult.NotFound.Instance;
                        else throw new UnreachableCodeException();
                    }
                }
                else
                {
                    throw new UnreachableCodeException();
                }
            }

            // 확정된 context로 업데이트
            var updateContext = UpdateContext.Make();
            updateContext.Update(scopeContext, matchedFunc.Context);

            return new FuncMatchIndexResult.Success(matchedFunc.Index, matchedFunc.Result.TypeArgs, matchedFunc.Result.Args);
        }

        // entry 2
        public static FuncMatchResult? Match(
            ScopeContext scopeContext,
            TypeEnv outerTypeEnv,
            ImmutableArray<FuncParameter> paramTypes,
            int? variadicParamIndex,
            ImmutableArray<IType> typeArgs,
            ImmutableArray<S.Argument> sargs)
        {
            var result = MatchCallableCore(scopeContext, outerTypeEnv, paramTypes, variadicParamIndex, typeArgs, sargs);
            if (result != null)
                return new FuncMatchResult(result.Value.TypeArgs, result.Value.Args);
            else
                return null;
        }

        FuncMatcher(ImmutableArray<FuncParameter> paramInfos, ImmutableArray<IType> typeArgs, ImmutableArray<FuncMatcherArgument> expandedArgs, TypeResolver typeResolver, TypeEnv outerTypeEnv)
        {
            this.paramInfos = paramInfos;
            this.typeArgs = typeArgs;
            this.expandedArgs = expandedArgs;
            this.typeResolver = typeResolver;
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

        static MatchCallableResult? MatchCallableCore(
            ScopeContext scopeContext,
            TypeEnv outerTypeEnv,
            ImmutableArray<FuncParameter> paramTypes,
            int? variadicParamIndex,
            ImmutableArray<IType> typeArgs,
            ImmutableArray<S.Argument> sargs) // nothrow
        {
            try
            {
                // 1. 아규먼트에서 params를 확장시킨다 (params에는 타입힌트를 적용하지 않고 먼저 평가한다)
                var expandedArgs = ExpandArguments(scopeContext, sargs);
                var typeResolver = new NullTypeResolver(typeArgs); // 일단 이걸로

                var inst = new FuncMatcher(paramTypes, typeArgs, expandedArgs, typeResolver, outerTypeEnv);

                // 2. funcInfo에서 params 앞부분과 뒷부분으로 나누어서 인자 체크를 한다
                if (variadicParamIndex != null)
                    return inst.MatchFuncWithParams(variadicParamIndex.Value);
                else
                    return inst.MatchFuncWithoutParams();
            }
            catch (FuncMatcherFatalException)
            {
                return null;
            }
        }

        // params를 확장한 Argument
        abstract class FuncMatcherArgument
        {
            // preanalyze할수도 있고, 여기서 시작할수도 있다
            public abstract void DoAnalyze(FuncParameter? expectType); // throws FuncMatchFatalException
            public abstract IType GetArgType();
            public abstract R.Argument? GetRArgument();

            // 기본 아규먼트 
            public class Normal : FuncMatcherArgument
            {
                ScopeContext context;
                S.Exp sexp;
                R.Exp? exp;

                public Normal(ScopeContext context, S.Exp sexp)
                {
                    this.context = context;
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
                            var hint = paramInfo.Value.Type;
                            var argResult = ExpVisitor.TranslateAsExp(sexp, context, hint);
                            exp = BodyMisc.TryCastExp_Exp(argResult, paramInfo.Value.Type);

                            if (exp == null)
                                throw new FuncMatcherFatalException();
                        }
                    }
                    else // none, EnumConstructor
                    {
                        exp = ExpVisitor.TranslateAsExp(sexp, context, hintType: null);
                    }
                }

                public override IType GetArgType()
                {
                    Debug.Assert(exp != null);
                    return exp.GetExpType();
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
                IType type;

                public ParamsHead(R.Exp exp, int paramCount, IType type)
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

                public override IType GetArgType()
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
                IType type;

                public ParamsRest(IType type)
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

                public override IType GetArgType()
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
                ScopeContext context;
                S.Exp exp;
                (R.Loc Loc, IType Type)? locResult;

                public Ref(ScopeContext context, S.Exp exp)
                {
                    this.context = context;
                    this.exp = exp;
                    this.locResult = null;
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
                        var argResult = ExpVisitor.TranslateAsLoc(exp, context, hintType: null, bWrapExpAsLoc: false);
                        if (argResult == null)
                            throw new FuncMatcherFatalException();

                        this.locResult = argResult.Value;
                    }
                }

                public override IType GetArgType()
                {
                    Debug.Assert(locResult != null);
                    return locResult.Value.Type;
                }

                public override R.Argument? GetRArgument()
                {
                    Debug.Assert(locResult != null);
                    return new R.Argument.Ref(locResult.Value.Loc);
                }
            }
        }

        static ImmutableArray<FuncMatcherArgument> ExpandArguments(ScopeContext context, ImmutableArray<S.Argument> sargs) // throws FuncMatcherFatalException
        {
            var args = ImmutableArray.CreateBuilder<FuncMatcherArgument>();
            foreach (var sarg in sargs)
            {
                if (sarg is S.Argument.Normal sNormalArg)
                {
                    args.Add(new FuncMatcherArgument.Normal(context, sNormalArg.Exp));
                }
                else if (sarg is S.Argument.Params sParamsArg)
                {
                    // expanded argument는 먼저 타입을 알아내야 한다
                    R.Exp exp;
                    try
                    {
                        exp = ExpVisitor.TranslateAsExp(sParamsArg.Exp, context, hintType: null);
                    }
                    catch (AnalyzerFatalException)
                    {
                        throw new FuncMatcherFatalException();
                    }

                    var expType = exp.GetExpType();

                    if (expType is TupleType tupleExpType)
                    {
                        int memberVarCount = tupleExpType.GetMemberVarCount();

                        // head
                        if (0 < memberVarCount)
                        {
                            var memberVar = tupleExpType.GetMemberVar(0);
                            var arg = new FuncMatcherArgument.ParamsHead(exp, memberVarCount, memberVar.GetDeclType());
                            args.Add(arg);
                        }

                        // rest
                        for(int i = 1; i < memberVarCount; i++)
                        {
                            var memberVar = tupleExpType.GetMemberVar(i);
                            args.Add(new FuncMatcherArgument.ParamsRest(memberVar.GetDeclType()));
                        }
                    }
                    else
                    {
                        throw new FuncMatcherFatalException();
                    }
                }
                else if (sarg is S.Argument.Ref sRefArg)
                {
                    args.Add(new FuncMatcherArgument.Ref(context, sRefArg.Exp));
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
                typeResolver.AddConstraint(paramType.Type, arg.GetArgType());
            }
        }

        void MatchParamsArguments(int paramIndex, int argsBegin, int argsEnd) // throws FuncMatcherFatalException
        {
            var paramInfo = paramInfos[paramIndex];
            Debug.Assert(paramInfo.Kind == FuncParameterKind.Params);

            if (paramInfo.Type is TupleType tupleParamType)
            {
                int paramsCount = tupleParamType.GetMemberVarCount();

                if (paramsCount != argsEnd - argsBegin)
                {
                    throw new NotImplementedException(); // TODO: 에러 처리
                    // throw new FuncMatcherFatalException();
                }

                for (int i = 0; i < paramsCount; i++)
                {
                    var memberVar = tupleParamType.GetMemberVar(i);
                    var tupleElemType = memberVar.GetDeclType();
                    var arg = expandedArgs[argsBegin + i];

                    MatchArgument(new FuncParameter(FuncParameterKind.Default, tupleElemType, memberVar.GetName()), arg);

                    // MatchArgument마다 Constraint추가
                    typeResolver.AddConstraint(tupleElemType, arg.GetArgType());
                }
            }
            else // params T <=> (1, 2, "hi")
            {
                var argsLength = argsEnd - argsBegin;
                var elemBuilder = ImmutableArray.CreateBuilder<TupleMemberVar>(argsLength);

                for (int i = 0; i < argsLength; i++)
                {
                    var arg = expandedArgs[i];
                    MatchArgument_UnknownParamType(arg);

                    var argType = arg.GetArgType();
                    var name = new Name.Normal($"Item{i}");

                    elemBuilder.Add(new TupleMemberVar(argType, name)); // unnamed tuple element
                }

                var argTupleType = new TupleType(elemBuilder.MoveToImmutable());

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
}
