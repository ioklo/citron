using Citron.Symbol;

namespace Citron.Analysis;

partial class GlobalContext
{
    static VoidTypeId voidId;
    static SymbolId boolId, intId, stringId;
    static DeclSymbolId boolDeclId, intDeclId, stringDeclId;

    static DeclSymbolId nullableDeclId, listDeclId;

    static GlobalContext()
    {
        voidId = new VoidTypeId();

        boolDeclId = new DeclSymbolId(new Name.Normal("System.Runtime"), null).Child(new Name.Normal("System")).Child(new Name.Normal("Boolean"));
        boolId = new SymbolId(new Name.Normal("System.Runtime"), null).Child(new Name.Normal("System")).Child(new Name.Normal("Boolean"));

        intDeclId = new DeclSymbolId(new Name.Normal("System.Runtime"), null).Child(new Name.Normal("System")).Child(new Name.Normal("Int32"));
        intId = new SymbolId(new Name.Normal("System.Runtime"), null).Child(new Name.Normal("System")).Child(new Name.Normal("Int32"));

        stringDeclId = new DeclSymbolId(new Name.Normal("System.Runtime"), null).Child(new Name.Normal("System")).Child(new Name.Normal("String"));
        stringId = new SymbolId(new Name.Normal("System.Runtime"), null).Child(new Name.Normal("System")).Child(new Name.Normal("String"));

        listDeclId = new DeclSymbolId(new Name.Normal("System.Runtime"), null).Child(new Name.Normal("System")).Child(new Name.Normal("List"), 1);
        nullableDeclId = new DeclSymbolId(new Name.Normal("System.Runtime"), null).Child(new Name.Normal("System")).Child(new Name.Normal("Nullable"), 1);
    }

}