using Gum.Collections;
using Pretune;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using M = Gum.CompileTime;

namespace Gum.IR0Translator
{
    [ImplementIEquatable]
    partial class InternalModuleClassInfo : IModuleClassInfo
    {
        M.Name name;
        ImmutableArray<string> typeParams;
        M.Type? baseType;
        ImmutableArray<M.Type> interfaceTypes;
        ImmutableArray<IModuleTypeInfo> types;
        ImmutableArray<IModuleFuncInfo> funcs;
        ImmutableArray<IModuleConstructorInfo> constructors;
        IModuleConstructorInfo? autoConstructor;
        ImmutableArray<IModuleMemberVarInfo> memberVars;

        ModuleTypeDict typeDict;
        ModuleFuncDict funcDict;

        public InternalModuleClassInfo(
            M.Name name, ImmutableArray<string> typeParams, 
            M.Type? baseType,
            ImmutableArray<M.Type> interfaceTypes,
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

        IModuleConstructorInfo? IModuleClassInfo.GetAutoConstructor()
        {
            return autoConstructor;
        }

        M.Type? IModuleClassInfo.GetBaseType()
        {
            return baseType;
        }

        ImmutableArray<IModuleConstructorInfo> IModuleClassInfo.GetConstructors()
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

        ImmutableArray<IModuleFuncInfo> IModuleClassInfo.GetMemberFuncs()
        {
            return funcs;
        }

        ImmutableArray<IModuleTypeInfo> IModuleClassInfo.GetMemberTypes()
        {
            return types;
        }

        ImmutableArray<IModuleMemberVarInfo> IModuleClassInfo.GetMemberVars()
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
