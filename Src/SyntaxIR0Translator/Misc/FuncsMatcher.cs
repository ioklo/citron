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

namespace Citron.Analysis;

// 어떤 Exp에서 타입 정보 등을 알아냅니다

record struct MatchedFunc(MatchCallableResult Result, int Index, ScopeContext Context);
// bExactMatch: TypeInference를 사용하지 않은 경우
record struct MatchCallableResult(bool bExactMatch, ImmutableArray<R.Exp> Args, ImmutableArray<IType> TypeArgs);

// entry1의 result        
abstract record class FuncMatchIndexResult
{
    public record class MultipleCandidates(ImmutableArray<MatchedFunc> MatchedFuncs) : FuncMatchIndexResult;

    public record class NotFound : FuncMatchIndexResult
    {   
        internal NotFound() { }
    }
        
    public record class Success(int Index, ImmutableArray<IType> TypeArgs, ImmutableArray<R.Exp> Args) : FuncMatchIndexResult;
}

static class FuncMatchIndexResults
{
    public static FuncMatchIndexResult.NotFound NotFound = new FuncMatchIndexResult.NotFound();
}

// entry2의 result    
record struct FuncMatchResult(ImmutableArray<IType> TypeArgs, ImmutableArray<R.Exp> Args);

struct FuncsMatcher
{   
    ImmutableArray<FuncParameter> funcParams;
    ImmutableArray<IType> typeArgs;
    ImmutableArray<S.Argument> argSyntaxes;
    TypeResolver typeResolver;

    TypeEnv outerTypeEnv;

    

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
            if (exactMatchResult.IsMultipleError(out var matchedFuncs))
            {
                return new FuncMatchIndexResult.MultipleCandidates(matchedFuncs);
            }
            else if (exactMatchResult.IsNotFound()) // empty
            {
                var restMatchResult = restCandidates.GetUniqueResult();
                if (!restMatchResult.IsFound(out matchedFunc))
                {
                    if (restMatchResult.IsMultipleError(out matchedFuncs)) return new FuncMatchIndexResult.MultipleCandidates(matchedFuncs);
                    else if (restMatchResult.IsNotFound()) return FuncMatchIndexResults.NotFound;
                    else throw new UnreachableException();
                }
            }
            else
            {
                throw new UnreachableException();
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
        bool bVariadic,
        ImmutableArray<IType> typeArgs,
        ImmutableArray<S.Argument> sargs) // nothrow
    {   
        var typeResolver = new NullTypeResolver(typeArgs); // 일단 이걸로
        var inst = new FuncMatcher(paramTypes, typeArgs, sargs, typeResolver, outerTypeEnv);

        // 2. funcInfo에서 params 앞부분과 뒷부분으로 나누어서 인자 체크를 한다
        if (bVariadic)
            return inst.MatchFuncWithParams();
        else
            return inst.MatchFuncWithoutParams();
    }

    // params를 확장한 Argument
    abstract record class FuncMatcherArgument
    {
        // 기본 아규먼트 
        public record class Normal(ScopeContext context, S.Exp sexp) : FuncMatcherArgument;

        // Params가 확장된 아규먼트
        public record class ParamsHead(R.Exp exp, int paramCount, IType type) : FuncMatcherArgument;
        public record class ParamsRest(IType type) : FuncMatcherArgument;
    }

    //static TranslationResult<ImmutableArray<FuncMatcherArgument>> ExpandArguments(ScopeContext context, ImmutableArray<S.Argument> sargs)
    //{
    //    var args = ImmutableArray.CreateBuilder<FuncMatcherArgument>();
    //    foreach (var sarg in sargs)
    //    {
    //        if (sarg is S.Argument.Normal sNormalArg)      // F(1)
    //        {
    //            args.Add(new FuncMatcherArgument.Normal(context, sNormalArg.Exp));
    //        }
    //        else if (sarg is S.Argument.Params sParamsArg) // F(..., params x, ...)
    //        {
    //            // expanded argument는 먼저 타입을 알아내야 한다                
    //            var expResult = ExpIR0ExpTranslator.Translate(sParamsArg.Exp, context, hintType: null, bDerefIfTypeIsRef: true);
    //            if (!expResult.IsValid(out var exp))
    //                return TranslationResult.Error<ImmutableArray<FuncMatcherArgument>>();

    //            var expType = exp.GetExpType();

    //            if (expType is TupleType tupleExpType)
    //            {
    //                int memberVarCount = tupleExpType.GetMemberVarCount();

    //                // head
    //                if (0 < memberVarCount)
    //                {
    //                    var memberVar = tupleExpType.GetMemberVar(0);
    //                    var arg = new FuncMatcherArgument.ParamsHead(exp, memberVarCount, memberVar.GetDeclType());
    //                    args.Add(arg);
    //                }

    //                // rest
    //                for(int i = 1; i < memberVarCount; i++)
    //                {
    //                    var memberVar = tupleExpType.GetMemberVar(i);
    //                    args.Add(new FuncMatcherArgument.ParamsRest(memberVar.GetDeclType()));
    //                }
    //            }
    //            else
    //            {
    //                return TranslationResult.Error<ImmutableArray<FuncMatcherArgument>>();
    //            }
    //        }
    //    }

    //    return TranslationResult.Valid(args.ToImmutable());
    //}

    

    // argument 부분의 params를 하나씩 풀어서 리턴한다
    // 1. 성공
    // 2. 단순 끝 -> null
    // 3. 매칭을 종료해야할 만큼의 Fatal 에러 Error
    
    bool MatchArgument(FuncParameter param, R.Exp arg, TypeResolver typeResolver)
    {
        
        typeResolver.AddConstraint(appliedParam.Type, exp.GetExpType());

        return true;

    }

    // 1. 매칭 성공
    // 2. 매칭 실패
    // 3. 매칭이고 뭐고 종료 Fatal
    TranslationResult<MatchCallableResult?> MatchFuncWithoutParams()
    {
        var rargsBuilder = ImmutableArray.CreateBuilder<R.Exp>();

        // 앞에서부터 하나씩
        for (int curParamIndex = 0; curParamIndex < funcParams.Length; curParamIndex++)
        {
            var funcParam = funcParams[curParamIndex];

            var argResult = GetNextArgument(funcParam);
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

    MatchCallableResult? MatchFuncWithParams()
    {
        //                  v
        // param(4) : 0 1 2 3
        // args(6)  : 0 1 2 3 4 5                    
        // wargs(2) : 0 1

        // front(Params,Args) : [0, v) 
        // backParams         : [v + 1 ~ parameters.Length)
        // backArgs           : [args.Length - (parameters.Length - v - 1), args.Length)
        // front.end <= backArgs.begin 이어야만 한다

        int v = variadicParamIndex;
        int argsEnd = expandedArgs.Length;
        int paramsEnd = funcParams.Length;

        int backArgsBegin = argsEnd - (paramsEnd - v - 1);

        if (backArgsBegin < v)
            return null; // 매치가 안된게 에러는 아니므로 그냥 리턴

        // 앞부분
        if (!MatchPartialArguments(0, v, 0, v))
            return null;

        // 중간 params 부분
        if (!MatchParamsArguments(v, v, backArgsBegin))
            return null;

        // 뒷부분
        if (!MatchPartialArguments(v + 1, paramsEnd, backArgsBegin, argsEnd))
            return null;

        // typeargs 만들기
        var resolvedTypeArgs = typeResolver.Resolve();
        var bExactMatch = funcParams.Length == typeArgs.Length;
        var rargs = MakeRArgs();

        return new MatchCallableResult(true, bExactMatch, rargs, resolvedTypeArgs);
    }

    #region Layer1                
    bool MatchPartialArguments(int paramsBegin, int paramsEnd, int argsBegin, int argsEnd, ImmutableArray<R.Exp>.Builder rargsBuilder) // throws FuncMatcherFatalException
    {
        Debug.Assert(paramsEnd - paramsBegin == argsEnd - argsBegin);
        int paramsCount = paramsEnd - paramsBegin;

        for (int i = 0; i < paramsCount; i++)
        {
            var paramType = funcParams[paramsBegin + i];
            var arg = expandedArgs[argsBegin + i];

            if (!MatchArgument(paramType, arg, rargsBuilder, typeResolver))
                return false;

            // MatchArgument마다 Constraint추가
            // typeResolver.AddConstraint(paramType.Type, arg.GetArgType());
        }

        return true;
    }

    bool MatchParamsArguments(int paramIndex, int argsBegin, int argsEnd, ImmutableArray<R.Exp>.Builder rargsBuilder, TypeResolver typeResolver)
    {
        var paramInfo = funcParams[paramIndex];
        Debug.Assert(paramInfo.Kind == FuncParameterKind.Params);

        if (paramInfo.Type is TupleType tupleParamType)
        {
            int paramsCount = tupleParamType.GetMemberVarCount();

            if (paramsCount != argsEnd - argsBegin)
                return false;

            for (int i = 0; i < paramsCount; i++)
            {
                var memberVar = tupleParamType.GetMemberVar(i);
                var tupleElemType = memberVar.GetDeclType();
                var arg = expandedArgs[argsBegin + i];

                if (!MatchArgument(new FuncParameter(FuncParameterKind.Default, tupleElemType, memberVar.GetName()), arg, rargsBuilder, typeResolver))
                    return false;
            }
        }
        else // params T <=> (1, 2, "hi")
        {
            var argsLength = argsEnd - argsBegin;
            var elemBuilder = ImmutableArray.CreateBuilder<TupleMemberVar>(argsLength);

            for (int i = 0; i < argsLength; i++)
            {
                var arg = expandedArgs[i];
                if (!MatchArgument_UnknownParamType(arg, elemBuilder, rargsBuilder, typeResolver))
                    return false;
            }

            var argTupleType = new TupleType(elemBuilder.MoveToImmutable());

            // Constraint 추가
            typeResolver.AddConstraint(paramInfo.Type, argTupleType);
        }
    }

    // Layer 1
    ImmutableArray<R.Exp> MakeRArgs()
    {
        var builder = ImmutableArray.CreateBuilder<R.Exp>();

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
    bool MatchArgument_UnknownParamType(FuncMatcherArgument arg, ImmutableArray<R.Exp>.Builder rargsBuilder, TypeResolver typeResolver)
    {
        switch(arg)
        {
            case FuncMatcherArgument.Normal normalArg:
            {
                var expResult = ExpIR0ExpTranslator.Translate(normalArg.sexp, normalArg.context, hintType: null, bDerefIfTypeIsRef: true);
                if (!expResult.IsValid(out var exp))
                    return false;

                rargsBuilder.Add(exp);
                return true;
            }

            case FuncMatcherArgument.ParamsHead paramHeadArg:
                throw new NotImplementedException(); // 에러?

            case FuncMatcherArgument.ParamsRest paramRestArg:
                throw new NotImplementedException(); // 에러?
        }

        var argType = arg.GetArgType();
        var name = new Name.Normal($"Item{i}");

        elemBuilder.Add(new TupleMemberVar(argType, name)); // unnamed tuple element
        return true;
    }
    #endregion
}


