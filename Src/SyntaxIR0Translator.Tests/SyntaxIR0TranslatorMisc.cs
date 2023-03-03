using Citron.Infra;
using Citron.Symbol;

namespace Citron.Test;

static class SyntaxIR0TranslatorMisc
{
    public static Name NormalName(string text)
    {
        return new Name.Normal(text);
    }

    public static IType? MakeOpenType(this ITypeDeclSymbol typeDSymbol, SymbolFactory factory)
    {
        return typeDSymbol.MakeOpenSymbol(factory).MakeType();
    }

    public static void AssertEquals<TClass>(TClass? x, TClass? y)
        where TClass : class, ICyclicEqualityComparableClass<TClass>
    {
        var context = new CyclicEqualityCompareContext();
        context.CompareClass(x, y);
    }
}
