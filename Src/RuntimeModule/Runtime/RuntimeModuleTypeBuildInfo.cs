using Citron.CompileTime;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Citron.Runtime
{
    abstract class RuntimeModuleTypeBuildInfo
    {
        ItemId? outerTypeId;
        ItemId id;
        IEnumerable<string> typeParams;
        ITypeSymbol? baseTypeValue;
        Func<Value> defaultValueFactory;

        public ItemId? GetOuterTypeId() => outerTypeId;
        public ItemId GetId() => id;
        public IEnumerable<string> GetTypeParams() => typeParams;
        public ITypeSymbol? GetBaseTypeValue() => baseTypeValue;
        public Func<Value> GetDefaultValueFactory() => defaultValueFactory;

        public RuntimeModuleTypeBuildInfo(ItemId? outerTypeId, ItemId id, IEnumerable<string> typeParams, ITypeSymbol? baseTypeValue, Func<Value> defaultValueFactory)
        {
            this.outerTypeId = outerTypeId;
            this.id = id;
            this.typeParams = typeParams;
            this.baseTypeValue = baseTypeValue;
            this.defaultValueFactory = defaultValueFactory;
        }

        public abstract class Class : RuntimeModuleTypeBuildInfo
        {   
            public Class(ItemId? outerTypeId, ItemId id, IEnumerable<string> typeParams, ITypeSymbol? baseTypeValue, Func<Value> defaultValueFactory)
                : base(outerTypeId, id, typeParams, baseTypeValue, defaultValueFactory)
            {   
            }
        }

        public abstract class Struct : RuntimeModuleTypeBuildInfo
        {
            public Struct(ItemId? outerTypeId, ItemId id, IEnumerable<string> typeParams, ITypeSymbol? baseTypeValue, Func<Value> defaultValueFactory)
                : base(outerTypeId, id, typeParams, baseTypeValue, defaultValueFactory)
            {
            }
        }

        public abstract void Build(RuntimeModuleTypeBuilder builder);
    }
}
