using System;

using Citron.Collections;
using Citron.Symbol;
using S = Citron.Syntax;
using R = Citron.IR0;

namespace Citron.Analysis;

struct FuncsMatcher
{
    public record struct MatchResultEntry(int Index, ImmutableArray<IType> TypeArgs, ImmutableArray<R.Argument> Args);
    public record struct Candidate(ImmutableArray<FuncParameter> Parameters, bool bLastParamVariadic);

    // F<int, short>(arg0, arg1) 을 지칭할때
    // candidate가 F(int x, bool b), F<Y>(T, Y y)
    public static ImmutableArray<MatchResultEntry> Match(
        TypeEnv outerTypeEnv,                   //
        ImmutableArray<Candidate> candidates,   // 
        ImmutableArray<IType> partialTypeArgs,  // <int, short>
        ImmutableArray<S.Argument> argSyntaxes, // arg0, arg1
        ScopeContext context)
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

            var c = new FuncsMatcher.Candidate(parametersBuilder.MoveToImmutable(), ds.IsLastParameterVariadic());
            builder.Add(c);
        }

        return builder.MoveToImmutable();
    }

}
