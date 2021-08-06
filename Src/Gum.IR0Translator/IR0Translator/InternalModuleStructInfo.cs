using Gum.Collections;
using Pretune;
using System.Collections.Generic;
using M = Gum.CompileTime;

namespace Gum.IR0Translator
{
    [ImplementIEquatable]
    partial class InternalModuleStructInfo : IModuleStructInfo
    {
        M.Name name;
        ImmutableArray<string> typeParams;
        M.Type? baseType;
        ImmutableArray<IModuleTypeInfo> types;
        ImmutableArray<IModuleFuncInfo> funcs;
        ImmutableArray<IModuleConstructorInfo> constructors;
        IModuleConstructorInfo? autoConstructor;
        ImmutableArray<IModuleMemberVarInfo> memberVars;

        ModuleTypeDict typeDict;
        ModuleFuncDict funcDict;

        public InternalModuleStructInfo(
            M.Name name, ImmutableArray<string> typeParams, M.Type? baseType,
            IEnumerable<IModuleTypeInfo> types,
            IEnumerable<IModuleFuncInfo> funcs,
            ImmutableArray<IModuleConstructorInfo> constructors,
            IModuleConstructorInfo? autoConstructor,
            ImmutableArray<IModuleMemberVarInfo> memberVars)
        {
            this.name = name;
            this.typeParams = typeParams;
            this.baseType = baseType;
            this.types = types.ToImmutableArray();
            this.funcs = funcs.ToImmutableArray();
            this.constructors = constructors;
            this.autoConstructor = autoConstructor;
            this.memberVars = memberVars;

            this.typeDict = new ModuleTypeDict(types);
            this.funcDict = new ModuleFuncDict(funcs);
        }

        IModuleConstructorInfo? IModuleStructInfo.GetAutoConstructor()
        {
            return autoConstructor;
        }

        M.Type? IModuleStructInfo.GetBaseType()
        {
            return baseType;
        }

        ImmutableArray<IModuleConstructorInfo> IModuleStructInfo.GetConstructors()
        {
            return constructors;
        }

        IModuleFuncInfo? IModuleFuncContainer.GetFunc(M.Name name, int typeParamCount, M.ParamTypes paramTypes)
        {
            return funcDict.Get(name, typeParamCount, paramTypes);
        }

        ImmutableArray<IModuleFuncInfo> IModuleFuncContainer.GetFuncs(M.Name name, int minTypeParamCount)
        {
            return funcDict.Get(name, minTypeParamCount);
        }

        ImmutableArray<IModuleFuncInfo> IModuleStructInfo.GetMemberFuncs()
        {
            return funcs;
        }

        ImmutableArray<IModuleTypeInfo> IModuleStructInfo.GetMemberTypes()
        {
            return types;
        }

        ImmutableArray<IModuleMemberVarInfo> IModuleStructInfo.GetMemberVars()
        {
            return memberVars;
        }

        M.Name IModuleItemInfo.GetName()
        {
            return name;
        }

        IModuleTypeInfo? IModuleTypeContainer.GetType(M.Name name, int typeParamCount)
        {
            return typeDict.Get(name, typeParamCount);
        }

        ImmutableArray<string> IModuleItemInfo.GetTypeParams()
        {
            return typeParams;
        }
    }
}