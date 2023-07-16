using System;
using System.Collections.Generic;
using System.Diagnostics;

using Citron.Collections;
using Citron.Symbol;
using S = Citron.Syntax;
using R = Citron.IR0;

using static Citron.Analysis.SyntaxAnalysisErrorCode;
using System.Reflection.Metadata;

namespace Citron.Analysis;

struct FuncMatcher 
{
    public record struct MatchResult(bool bExact, ImmutableArray<R.Argument> Args);

    // 결과: Match가 되었느냐만 따진다
    public static MatchResult? Match(
        ScopeContext context, 
        TypeEnv outerTypeEnv, 
        ImmutableArray<FuncParameter> parameters, 
        bool bVariadic, 
        ImmutableArray<IType> partialTypeArgs, 
        ImmutableArray<S.Argument> argSyntaxes, 
        TypeResolver typeResolver)
    {
        throw new NotImplementedException();
    }
}
