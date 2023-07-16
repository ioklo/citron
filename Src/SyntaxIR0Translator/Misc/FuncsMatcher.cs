using System;

using Citron.Collections;
using Citron.Symbol;
using S = Citron.Syntax;
using R = Citron.IR0;

namespace Citron.Analysis;

struct FuncsMatcher
{
    public record struct MatchResultEntry(int Index, ImmutableArray<IType> TypeArgs, ImmutableArray<R.Argument> Args);
    public record struct Candidate(bool bVariadic, ImmutableArray<FuncParameter> Parameters);

    public static ImmutableArray<MatchResultEntry> Match(
        ScopeContext context,
        TypeEnv outerTypeEnv,
        ImmutableArray<Candidate> candidates, 
        ImmutableArray<S.Argument> argSyntaxes, 
        ImmutableArray<IType> partialTypeArgs)
    {
        throw new NotImplementedException();
    }
}

static class FuncsMatcherExtensions
{
    public static ImmutableArray<FuncsMatcher.Candidate> MakeCandidates<TFuncDeclSymbol>(int count, Func<int, TFuncDeclSymbol> getter)
        where TFuncDeclSymbol : IFuncDeclSymbol
    {
        var builder = ImmutableArray.CreateBuilder<FuncsMatcher.Candidate>(count);

        for (int i = 0; i < count; i++)
        {
            var ds = getter.Invoke(i);

            var parameterCount = ds.GetParameterCount();
            var parametersBuilder = ImmutableArray.CreateBuilder<FuncParameter>(parameterCount);
            for (int j = 0; j < parameterCount; j++)
                parametersBuilder.Add(ds.GetParameter(j));

            var c = new FuncsMatcher.Candidate(ds.IsLastParameterVariadic(), parametersBuilder.MoveToImmutable());
            builder.Add(c);
        }

        return builder.MoveToImmutable();
    }

}
