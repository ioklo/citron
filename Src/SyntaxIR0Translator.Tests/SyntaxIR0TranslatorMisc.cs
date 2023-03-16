using Citron.Infra;
using Citron.Collections;
using Citron.Analysis;
using Citron.Log;
using Citron.Symbol;
using Xunit;

using S = Citron.Syntax;

using static Citron.Infra.Misc;


namespace Citron.Test;

static class SyntaxIR0TranslatorMisc
{
    public static IType? MakeOpenType(this ITypeDeclSymbol typeDSymbol, SymbolFactory factory)
    {
        return typeDSymbol.MakeOpenSymbol(factory).MakeType();
    }

    public static void AssertEquals<TClass>(TClass? expected, TClass? actual)
        where TClass : class, ICyclicEqualityComparableClass<TClass>
    {
        var context = new CyclicEqualityCompareContext();
        context.CompareClass(expected, actual);
    }

    public static void VerifyError(IEnumerable<ILog> logs, SyntaxAnalysisErrorCode code, S.ISyntaxNode node)
    {
        var result = logs.OfType<SyntaxAnalysisErrorLog>()
            .Any(error => error.Code == code && error.Node == node);

        Assert.True(result, $"Errors doesn't contain (Code: {code}, Node: {node})");
    }
}
