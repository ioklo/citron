using System;
using Citron.Symbol;
using Citron.Collections;
using static Citron.Infra.Misc;

namespace Citron.Analysis;

partial class GlobalContext
{
    partial struct RuntimeTypeComponent
    {
        static SymbolId systemNSId; // 
        static SymbolId boolId, intId, stringId;

        static DeclSymbolId nullableDeclId, listDeclId;

        static RuntimeTypeComponent()
        {
            systemNSId = new SymbolId(new Name.Normal("System.Runtime"), null).Child(new Name.Normal("System"));

            boolId = systemNSId.Child(new Name.Normal("Boolean"));
            intId = systemNSId.Child(new Name.Normal("Int32"));
            stringId = systemNSId.Child(new Name.Normal("String"));

            listDeclId = new DeclSymbolId(new Name.Normal("System.Runtime"), null).Child(new Name.Normal("System")).Child(new Name.Normal("List"), 1);
            nullableDeclId = new DeclSymbolId(new Name.Normal("System.Runtime"), null).Child(new Name.Normal("System")).Child(new Name.Normal("Nullable"), 1);
        }
    }

    partial struct RuntimeTypeComponent
    {
        TypeLoader loader;

        IType voidType;
        IType boolType;
        IType intType;
        IType stringType;

        RuntimeTypeComponent(SymbolFactory factory, ImmutableArray<ModuleDeclSymbol> moduleDecls)
        {
            var loader = new TypeLoader(new SymbolLoader(factory, moduleDecls));

            this.voidType = new VoidType();
            this.boolType = loader.Load(boolId);
            this.intType = loader.Load(intId);
            this.stringType = loader.Load(stringId);
        }

        public IType GetVoidType()
        {
            return voidType;
        }

        public IType GetBoolType()
        {
            return boolType;
        }

        public IType GetIntType()
        {
            return intType;
        }

        public IType GetStringType()
        {
            return stringType;
        }

        public IType GetListIterType(IType? itemType)
        {
            throw new NotImplementedException();
        }

        public IType GetListType(IType itemType)
        {
            var typeArgs = Arr(itemType.GetTypeId());
            var listId = systemNSId.Child(new Name.Normal("List"), typeArgs);
            return loader.Load(listId);
        }
    }

    public IType GetVoidType() => runtimeTypeComponent.GetVoidType();
    public IType GetBoolType() => runtimeTypeComponent.GetBoolType();
    public IType GetIntType() => runtimeTypeComponent.GetIntType();
    public IType GetStringType() => runtimeTypeComponent.GetStringType();
    public IType GetListIterType(IType? itemType) => runtimeTypeComponent.GetListIterType(itemType);
    public IType GetListType(IType itemType) => runtimeTypeComponent.GetListType(itemType);
}