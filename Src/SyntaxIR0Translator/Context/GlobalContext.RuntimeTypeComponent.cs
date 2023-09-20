using System;
using Citron.Symbol;
using Citron.Collections;
using static Citron.Infra.Misc;

namespace Citron.Analysis;

partial class GlobalContext
{
    public partial struct RuntimeTypeComponent
    {
        static SymbolId systemNSId; // 

        static DeclSymbolId nullableDeclId, listDeclId;

        static RuntimeTypeComponent()
        {
            systemNSId = new SymbolId(new Name.Normal("System.Runtime"), null).Child(new Name.Normal("System"));

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

        public RuntimeTypeComponent(SymbolFactory factory, ImmutableArray<ModuleDeclSymbol> moduleDecls)
        {
            this.loader = new TypeLoader(new SymbolLoader(factory, moduleDecls));

            this.voidType = new VoidType();
            this.boolType = loader.Load(TypeIds.Bool);
            this.intType = loader.Load(TypeIds.Int);
            this.stringType = loader.Load(TypeIds.String);
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
            return loader.Load(new SymbolTypeId(IsLocal: false, listId));
        }
    }

    public IType GetVoidType() => runtimeTypeComponent.GetVoidType();
    public IType GetBoolType() => runtimeTypeComponent.GetBoolType();
    public IType GetIntType() => runtimeTypeComponent.GetIntType();
    public IType GetStringType() => runtimeTypeComponent.GetStringType();
    public IType GetListIterType(IType? itemType) => runtimeTypeComponent.GetListIterType(itemType);
    public IType GetListType(IType itemType) => runtimeTypeComponent.GetListType(itemType);
}