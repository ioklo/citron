using Citron.IR0;
using Citron.Symbol;

using static Citron.Infra.Misc;
using static Citron.Test.Misc;

namespace Citron.Test;

public record struct TestPreparations(Name ModuleName, SymbolFactory SymbolFactory, IR0Factory IR0Factory, ModuleDeclSymbol RuntimeModuleD)
{
    public static TestPreparations Prepare()
    {
        var moduleName = NormalName("TestModule");
        var factory = new SymbolFactory();

        // runtime module
        var runtimeModuleD = new ModuleDeclSymbol(NormalName("System.Runtime"), bReference: true);
        var runtimeModuleSymbol = factory.MakeModule(runtimeModuleD);

        // system namespace
        var systemNSD = new NamespaceDeclSymbol(runtimeModuleD, NormalName("System"));
        runtimeModuleD.AddNamespace(systemNSD);
        var systemNSSymbol = factory.MakeNamespace(runtimeModuleSymbol, systemNSD);

        IType MakeBoolType()
        {
            var boolD = new StructDeclSymbol(systemNSD, Accessor.Public, NormalName("Bool"), typeParams: default);
            boolD.InitBaseTypes(baseStruct: null, interfaces: default);
            systemNSD.AddType(boolD);

            ITypeSymbol boolSymbol = factory.MakeStruct(systemNSSymbol, boolD, typeArgs: default);
            return boolSymbol.MakeType(bLocalInterface: false);
        }
        var boolType = MakeBoolType();

        IType MakeIntType()
        {
            var intD = new StructDeclSymbol(systemNSD, Accessor.Public, NormalName("Int32"), typeParams: default);
            intD.InitBaseTypes(baseStruct: null, interfaces: default);
            systemNSD.AddType(intD);
            ITypeSymbol intSymbol = factory.MakeStruct(systemNSSymbol, intD, typeArgs: default);
            return intSymbol.MakeType(bLocalInterface: false);
        }
        var intType = MakeIntType();

        IType MakeStringType()
        {
            var stringD = new ClassDeclSymbol(systemNSD, Accessor.Public, NormalName("String"), typeParams: default);
            stringD.InitBaseTypes(baseClass: null, interfaces: default);
            systemNSD.AddType(stringD);
            ITypeSymbol stringSymbol = factory.MakeClass(systemNSSymbol, stringD, typeArgs: default);
            return stringSymbol.MakeType(bLocalInterface: false);
        }
        var stringType = MakeStringType();

        (IR0Factory.ListTypeConstructor, IR0Factory.ListIterTypeConstructor) MakeListTypeConstructor()
        {
            var listD = new ClassDeclSymbol(systemNSD, Accessor.Public, NormalName("List"), Arr(NormalName("TItem")));
            listD.InitBaseTypes(baseClass: null, interfaces: default); // 일단;
            systemNSD.AddType(listD);

            var listIterD = new ClassDeclSymbol(listD, Accessor.Public, NormalName("Iterator"), typeParams: default);
            listIterD.InitBaseTypes(baseClass: null, interfaces: default);
            listD.AddType(listIterD);

            return (itemType =>
            {
                return ((ITypeSymbol)factory.MakeClass(systemNSSymbol, listD, Arr(itemType))).MakeType(bLocalInterface: false);
            }, itemType =>
            {
                var listSymbol = ((ITypeSymbol)factory.MakeClass(systemNSSymbol, listD, Arr(itemType)));
                return ((ITypeSymbol)factory.MakeClass(listSymbol, listIterD, default)).MakeType(bLocalInterface: false);
            }
            );
        }
        var (listTypeConstructor, listIterTypeConstructor) = MakeListTypeConstructor();

        var r = new IR0Factory(moduleName, new VoidType(), boolType, intType, stringType, listTypeConstructor, listIterTypeConstructor);
        return new TestPreparations(moduleName, factory, r, runtimeModuleD);
    }
}
