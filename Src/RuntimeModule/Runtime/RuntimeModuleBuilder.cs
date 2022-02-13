using Citron.CompileTime;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using Invoker = Citron.Runtime.RuntimeModuleMisc.Invoker;

namespace Citron.Runtime
{
    class RuntimeModuleBuilder
    {
        List<TypeInfo> typeInfos;
        List<FuncInfo> funcInfos;
        List<VarInfo> varInfos;

        List<NativeTypeInstantiator> typeInstantiators;
        List<NativeFuncInstantiator> funcInstantiators;

        public RuntimeModuleBuilder()
        {
            typeInfos = new List<TypeInfo>();
            funcInfos = new List<FuncInfo>();
            varInfos = new List<VarInfo>();

            typeInstantiators = new List<NativeTypeInstantiator>();
            funcInstantiators = new List<NativeFuncInstantiator>();
        }

        public void AddBuildInfo(RuntimeModuleTypeBuildInfo buildInfo)
        {
            RuntimeModuleTypeBuilder.BuildObject(this, buildInfo);
        }

        public void AddClassType(
            ItemId? outerTypeId,
            ItemId typeId,
            IEnumerable<string> typeParams,
            ITypeSymbol? baseTypeValue,
            IEnumerable<ItemId> memberTypeIds,
            IEnumerable<ItemId> memberFuncIds,
            IEnumerable<ItemId> memberVarIds,
            Func<Value> defaultValueFactory)
        {
            typeInfos.Add(new ClassInfo(outerTypeId, typeId, typeParams, baseTypeValue, memberTypeIds, memberFuncIds, memberVarIds));
            typeInstantiators.Add(new NativeTypeInstantiator(typeId, defaultValueFactory));
        }

        public void AddStructType(
            ItemId? outerTypeId,
            ItemId typeId,
            IEnumerable<string> typeParams,
            ITypeSymbol? baseTypeValue,
            IEnumerable<ItemId> memberTypeIds,
            IEnumerable<ItemId> memberFuncIds,
            IEnumerable<ItemId> memberVarIds,
            Func<Value> defaultValueFactory)
        {
            typeInfos.Add(new StructDecl(outerTypeId, typeId, typeParams, baseTypeValue, memberTypeIds, memberFuncIds, memberVarIds));
            typeInstantiators.Add(new NativeTypeInstantiator(typeId, defaultValueFactory));
        }

        public void AddFunc(
            ItemId? outerId,
            ItemId funcId,
            bool bSeqCall,
            bool bThisCall,
            IReadOnlyList<string> typeParams,
            ITypeSymbol retTypeValue,
            IEnumerable<ITypeSymbol> paramTypeValues,
            Invoker invoker)
        {
            funcInfos.Add(new FuncInfo(outerId, funcId, bSeqCall, bThisCall, typeParams, retTypeValue, paramTypeValues));
            funcInstantiators.Add(new NativeFuncInstantiator(funcId, bThisCall, invoker));
        }

        public void AddVar(ItemId? outerId, ItemId varId, bool bStatic, ITypeSymbol typeValue)
        {
            varInfos.Add(new VarInfo(outerId, varId, bStatic, typeValue));
        }
        
        public IEnumerable<TypeInfo> GetAllTypeInfos() => typeInfos;
        public IEnumerable<FuncInfo> GetAllFuncInfos() => funcInfos;
        public IEnumerable<VarInfo> GetAllVarInfos() => varInfos;
        public IEnumerable<NativeTypeInstantiator> GetAllTypeInstantiators() => typeInstantiators;
        public IEnumerable<NativeFuncInstantiator> GetAllFuncInstantiators() => funcInstantiators;
    }
}
