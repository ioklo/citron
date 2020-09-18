using Gum.CompileTime;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Gum.Runtime
{
    abstract class RuntimeModuleTypeBuildInfo
    {
        ModuleItemId? outerTypeId;
        ModuleItemId id;
        IEnumerable<string> typeParams;
        TypeValue? baseTypeValue;
        Func<Value> defaultValueFactory;

        public ModuleItemId? GetOuterTypeId() => outerTypeId;
        public ModuleItemId GetId() => id;
        public IEnumerable<string> GetTypeParams() => typeParams;
        public TypeValue? GetBaseTypeValue() => baseTypeValue;
        public Func<Value> GetDefaultValueFactory() => defaultValueFactory;

        public RuntimeModuleTypeBuildInfo(ModuleItemId? outerTypeId, ModuleItemId id, IEnumerable<string> typeParams, TypeValue? baseTypeValue, Func<Value> defaultValueFactory)
        {
            this.outerTypeId = outerTypeId;
            this.id = id;
            this.typeParams = typeParams;
            this.baseTypeValue = baseTypeValue;
            this.defaultValueFactory = defaultValueFactory;
        }

        public abstract class Class : RuntimeModuleTypeBuildInfo
        {   
            public Class(ModuleItemId? outerTypeId, ModuleItemId id, IEnumerable<string> typeParams, TypeValue? baseTypeValue, Func<Value> defaultValueFactory)
                : base(outerTypeId, id, typeParams, baseTypeValue, defaultValueFactory)
            {   
            }
        }

        public abstract class Struct : RuntimeModuleTypeBuildInfo
        {
            public Struct(ModuleItemId? outerTypeId, ModuleItemId id, IEnumerable<string> typeParams, TypeValue? baseTypeValue, Func<Value> defaultValueFactory)
                : base(outerTypeId, id, typeParams, baseTypeValue, defaultValueFactory)
            {
            }
        }

        public abstract void Build(RuntimeModuleTypeBuilder builder);
    }
}
