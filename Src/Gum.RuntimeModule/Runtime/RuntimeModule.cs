using Gum.CompileTime;
using System;
using System.Collections.Generic;
using Gum.Collections;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Gum.Runtime
{   
    //public class RuntimeModule : IRuntimeModule
    //{
    //    private ImmutableDictionary<ModuleItemId, ITypeInfo> typeInfos;
    //    private ImmutableDictionary<ModuleItemId, FuncInfo> funcInfos;
    //    private ImmutableDictionary<ModuleItemId, VarInfo> varInfos;
    //    private ImmutableDictionary<ModuleItemId, NativeTypeInstantiator> typeInstantiators;
    //    private ImmutableDictionary<ModuleItemId, NativeFuncInstantiator> funcInstantiators;

    //    private string homeDir;
    //    private string scriptDir;

    //    public const string MODULE_NAME = "System.Runtime";
    //    public string ModuleName => MODULE_NAME;

    //    public static ModuleItemId BoolId = ModuleItemId.Make("bool");
    //    public static ModuleItemId IntId = ModuleItemId.Make("int");
    //    public static ModuleItemId StringId = ModuleItemId.Make("string");
    //    public static ModuleItemId ListId = ModuleItemId.Make("List", 1);
    //    public static ModuleItemId EnumerableId = ModuleItemId.Make("Enumerable", 1);
    //    public static ModuleItemId EnumeratorId = ModuleItemId.Make("Enumerator", 1);

    //    public static ModuleItemId EnvironmentId = ModuleItemId.Make("Environment");
    //    public static ModuleItemId envId = ModuleItemId.Make("env");

    //    // TODO: localId와 globalId를 나눠야 할 것 같다. 내부에서는 LocalId를 쓰고, Runtime은 GlobalId로 구분해야 할 것 같다
    //    public bool GetTypeInfo(ModuleItemId id, [NotNullWhen(true)] out ITypeInfo? outTypeInfo)
    //    {
    //        return typeInfos.TryGetValue(id, out outTypeInfo);
    //    }

    //    public bool GetFuncInfo(ModuleItemId id, [NotNullWhen(true)] out FuncInfo? outFuncInfo)
    //    {
    //        return funcInfos.TryGetValue(id, out outFuncInfo);
    //    }

    //    public bool GetVarInfo(ModuleItemId id, [NotNullWhen(true)] out VarInfo? outVarInfo)
    //    {
    //        return varInfos.TryGetValue(id, out outVarInfo);
    //    }
        
    //    public RuntimeModule(string homeDir, string scriptDir)
    //    {
    //        var envTypeId = ModuleItemId.Make("Environment", 0);

    //        var moduleBuilder = new RuntimeModuleBuilder();

    //        moduleBuilder.AddBuildInfo(new EmptyStructBuildInfo(BoolId, () => new Value<bool>(false)));
    //        moduleBuilder.AddBuildInfo(new IntBuildInfo(this));
    //        moduleBuilder.AddBuildInfo(new EmptyClassBuildInfo(StringId, () => new ObjectValue(null)));
    //        moduleBuilder.AddBuildInfo(new EnumerableBuildInfo());
    //        moduleBuilder.AddBuildInfo(new EnumeratorBuildInfo());
    //        moduleBuilder.AddBuildInfo(new ListBuildInfo());
    //        moduleBuilder.AddBuildInfo(new EnvironmentBuildInfo());

    //        // objectInfo를 돌면서
    //        moduleBuilder.AddVar(null, envId, false, new NormalTypeValue(EnvironmentId));

    //        typeInfos = moduleBuilder.GetAllTypeInfos().ToImmutableDictionary(typeInfo => typeInfo.TypeId);
    //        funcInfos = moduleBuilder.GetAllFuncInfos().ToImmutableDictionary(funcInfo => funcInfo.FuncId);
    //        varInfos = moduleBuilder.GetAllVarInfos().ToImmutableDictionary(varInfo => varInfo.VarId);

    //        typeInstantiators = moduleBuilder.GetAllTypeInstantiators().ToImmutableDictionary(instantiator => instantiator.TypeId);
    //        funcInstantiators = moduleBuilder.GetAllFuncInstantiators().ToImmutableDictionary(instantiator => instantiator.FuncId);

    //        this.homeDir = homeDir;
    //        this.scriptDir = scriptDir;
    //    }

    //    class EmptyStructBuildInfo : RuntimeModuleTypeBuildInfo.Struct
    //    {
    //        public EmptyStructBuildInfo(ModuleItemId typeId, Func<Value> defaultValueFactory)
    //            : base(null, typeId, Enumerable.Empty<string>(), null, defaultValueFactory)
    //        {
    //        }

    //        public override void Build(RuntimeModuleTypeBuilder builder) 
    //        { 
    //        }
    //    }

    //    class EmptyClassBuildInfo : RuntimeModuleTypeBuildInfo.Class
    //    {
    //        public EmptyClassBuildInfo(ModuleItemId typeId, Func<Value> defaultValueFactory)
    //            : base(null, typeId, Enumerable.Empty<string>(), null, defaultValueFactory)
    //        {
    //        }

    //        public override void Build(RuntimeModuleTypeBuilder builder)
    //        {
    //        }
    //    }

    //    public string GetString(Value value)
    //    {
    //        if (value is ObjectValue objValue && objValue.Object is StringBuildInfo strObj) return strObj.Data;
    //        if (value is Value<int> intValue) return intValue.Data.ToString();
    //        if (value is Value<bool> boolValue) return boolValue.Data ? "true" : "false";

    //        // TODO: ObjectValue의 경우 ToString()을 찾는다
    //        throw new InvalidOperationException();
    //    }

    //    public void SetString(DomainService domainService, Value value, string s)
    //    {
    //        var stringInst = domainService.GetTypeInst(new NormalTypeValue(StringId));
    //        ((ObjectValue)value).SetObject(new StringBuildInfo(stringInst, s));
    //    }

    //    public void SetList(DomainService domainService, Value value, TypeValue elemTypeValue, List<Value> elems)
    //    {
    //        var listInst = domainService.GetTypeInst(new NormalTypeValue(ListId, TypeArgumentList.Make(elemTypeValue)));
    //        ((ObjectValue)value).SetObject(new ListObject(listInst, elems));
    //    }

    //    public void SetEnumerable(DomainService domainService, Value value, TypeValue elemTypeValue, IAsyncEnumerable<Value> asyncEnumerable)
    //    {
    //        var enumerableInst = domainService.GetTypeInst(new NormalTypeValue(EnumerableId, TypeArgumentList.Make(elemTypeValue)));
    //        ((ObjectValue)value).SetObject(new EnumerableObject(enumerableInst, asyncEnumerable));
    //    }        
        
    //    public Value MakeBool(bool b)
    //    {
    //        return new Value<bool>(b);
    //    }

    //    public Value MakeInt(int i)
    //    {
    //        return new Value<int>(i);
    //    }

    //    public Value MakeString(DomainService domainService, string str)
    //    {
    //        var stringInst = domainService.GetTypeInst(new NormalTypeValue(StringId));
    //        return new ObjectValue(new StringBuildInfo(stringInst, str));
    //    }

    //    public Value MakeList(DomainService domainService, TypeValue elemTypeValue, List<Value> elems)
    //    {
    //        var listInst = domainService.GetTypeInst(new NormalTypeValue(ListId, TypeArgumentList.Make(elemTypeValue)));

    //        return new ObjectValue(new ListObject(listInst, elems));
    //    }

    //    public int GetInt(Value value)
    //    {
    //        return ((Value<int>)value).Data;
    //    }
        
    //    public void SetInt(Value value, int i)
    //    {
    //        ((Value<int>)value).Data = i;
    //    }

    //    public bool GetBool(Value value)
    //    {
    //        return ((Value<bool>)value).Data;
    //    }

    //    public void SetBool(Value value, bool b)
    //    {
    //        ((Value<bool>)value).Data = b;
    //    }

    //    public void OnLoad(DomainService domainService)
    //    {
    //        // HomeDir?
    //        // ScriptDir?
    //        var homeDirValue = MakeString(domainService, homeDir);
    //        var scriptDirValue = MakeString(domainService, scriptDir);

    //        domainService.InitGlobalValue(envId, new ObjectValue(new EnvironmentObject(homeDirValue, scriptDirValue)));
    //    }

    //    public ObjectValue MakeNullObject()
    //    {
    //        return new ObjectValue(null);
    //    }

    //    public TypeInst GetTypeInst(DomainService domainService, NormalTypeValue ntv)
    //    {
    //        return typeInstantiators[ntv.TypeId].Instantiate(domainService, ntv);
    //    }

    //    public FuncInst GetFuncInst(DomainService domainService, FuncValue funcValue)
    //    {
    //        return funcInstantiators[funcValue.FuncId].Instantiate(domainService, funcValue);
    //    }
    //}
}
