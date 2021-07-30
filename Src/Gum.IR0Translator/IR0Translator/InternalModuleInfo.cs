using Gum.Collections;
using Gum.Infra;
using Pretune;
using System;
using System.Collections.Generic;
using M = Gum.CompileTime;

namespace Gum.IR0Translator
{
    partial class InternalModuleInfo : IPure, IModuleInfo
    {
        M.ModuleName moduleName;

        ModuleTypeDict typeDict;
        ModuleFuncDict funcDict;

        public InternalModuleInfo(M.ModuleName moduleName, IEnumerable<IModuleTypeInfo> types, IEnumerable<IModuleFuncInfo> funcs)
        {
            this.moduleName = moduleName;
            this.typeDict = new ModuleTypeDict(types);
            this.funcDict = new ModuleFuncDict(funcs);
        }

        public void EnsurePure()
        {
        }

        public M.ModuleName GetName()
        {
            return moduleName;
        }

        M.ModuleName IModuleInfo.GetName()
        {
            return moduleName;
        }

        IModuleNamespaceInfo? IModuleNamespaceContainer.GetNamespace(M.NamespaceName name)
        {
            throw new NotImplementedException();
        }

        ImmutableArray<IModuleFuncInfo> IModuleFuncContainer.GetFuncs(M.Name name, int minTypeParamCount)
        {
            return funcDict.Get(name, minTypeParamCount);
        }

        // 문제는
        IModuleTypeInfo? IModuleTypeContainer.GetType(M.Name name, int typeParamCount)
        {
            return typeDict.Get(name, typeParamCount);
        }

        IModuleFuncInfo? IModuleFuncContainer.GetFunc(M.Name name, int typeParamCount, M.ParamTypes paramTypes)
        {
            return funcDict.Get(name, typeParamCount, paramTypes);
        }
    }

    [AutoConstructor, ImplementIEquatable]
    partial class InternalModuleFuncInfo : IModuleFuncInfo
    {
        bool bInstanceFunc;
        bool bSeqFunc;
        bool bRefReturn;
        M.Type retType;        
        M.Name name;
        ImmutableArray<string> typeParams;
        ImmutableArray<M.Param> parameters;        

        M.Name IModuleItemInfo.GetName()
        {
            return name;
        }

        ImmutableArray<M.Param> IModuleCallableInfo.GetParameters()
        {
            return parameters;
        }

        M.ParamTypes IModuleCallableInfo.GetParamTypes()
        {
            return Misc.MakeParamTypes(parameters);
        }

        M.Type IModuleFuncInfo.GetReturnType()
        {
            return retType;
        }

        ImmutableArray<string> IModuleItemInfo.GetTypeParams()
        {
            return typeParams;
        }

        bool IModuleFuncInfo.IsInstanceFunc()
        {
            return bInstanceFunc;
        }

        bool IModuleFuncInfo.IsSequenceFunc()
        {
            return bSeqFunc;
        }
    }

    [AutoConstructor, ImplementIEquatable]
    partial class InternalModuleMemberVarInfo : IModuleMemberVarInfo
    {
        bool bStatic;
        M.Type declType;
        M.Name name;

        M.Type IModuleMemberVarInfo.GetDeclType()
        {
            return declType;
        }

        M.Name IModuleItemInfo.GetName()
        {
            return name;
        }

        ImmutableArray<string> IModuleItemInfo.GetTypeParams()
        {
            return ImmutableArray<string>.Empty;
        }

        bool IModuleMemberVarInfo.IsStatic()
        {
            return bStatic;
        }
    }
    
    partial class InternalModuleEnumInfo : IModuleEnumInfo
    {
        M.Name name;
        ImmutableArray<string> typeParams;
        ImmutableDictionary<M.Name, IModuleEnumElemInfo> elemDict;

        public InternalModuleEnumInfo(M.Name name, ImmutableArray<string> typeParams, ImmutableArray<InternalModuleEnumElemInfo> elemInfos)
        {
            this.name = name;
            this.typeParams = typeParams;

            var builder = ImmutableDictionary.CreateBuilder<M.Name, IModuleEnumElemInfo>();
            foreach(var elemInfo in elemInfos)
                builder.Add(((IModuleEnumElemInfo)elemInfo).GetName(), elemInfo);
            elemDict = builder.ToImmutable();
        }


        IModuleEnumElemInfo? IModuleEnumInfo.GetElem(M.Name memberName)
        {
            return elemDict.GetValueOrDefault(memberName);
        }

        IModuleFuncInfo? IModuleFuncContainer.GetFunc(M.Name name, int typeParamCount, M.ParamTypes paramTypes)
        {
            return null;
        }

        ImmutableArray<IModuleFuncInfo> IModuleFuncContainer.GetFuncs(M.Name name, int minTypeParamCount)
        {
            return ImmutableArray<IModuleFuncInfo>.Empty;
        }

        M.Name IModuleItemInfo.GetName()
        {
            return name;
        }

        IModuleTypeInfo? IModuleTypeContainer.GetType(M.Name name, int typeParamCount)
        {
            return null;
        }

        ImmutableArray<string> IModuleItemInfo.GetTypeParams()
        {
            return typeParams;
        }
    }

    [AutoConstructor, ImplementIEquatable]
    partial class InternalModuleEnumElemInfo : IModuleEnumElemInfo
    {
        M.Name name;
        ImmutableArray<IModuleMemberVarInfo> fieldInfos;

        ImmutableArray<IModuleMemberVarInfo> IModuleEnumElemInfo.GetFieldInfos()
        {
            return fieldInfos;
        }

        IModuleFuncInfo? IModuleFuncContainer.GetFunc(M.Name name, int typeParamCount, M.ParamTypes paramTypes)
        {
            return null;
        }

        ImmutableArray<IModuleFuncInfo> IModuleFuncContainer.GetFuncs(M.Name name, int minTypeParamCount)
        {
            return default;
        }

        M.Name IModuleItemInfo.GetName()
        {
            return name;
        }

        IModuleTypeInfo? IModuleTypeContainer.GetType(M.Name name, int typeParamCount)
        {
            return null;
        }

        ImmutableArray<string> IModuleItemInfo.GetTypeParams()
        {
            return default;
        }

        bool IModuleEnumElemInfo.IsStandalone()
        {
            return fieldInfos.Length == 0;
        }
    }

    [AutoConstructor, ImplementIEquatable]
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

    [AutoConstructor, ImplementIEquatable]
    partial class InternalModuleConstructorInfo : IModuleConstructorInfo
    {
        M.Name name;        
        ImmutableArray<M.Param> parameters;

        M.Name IModuleItemInfo.GetName()
        {
            return name;
        }

        ImmutableArray<M.Param> IModuleCallableInfo.GetParameters()
        {
            return parameters;
        }

        M.ParamTypes IModuleCallableInfo.GetParamTypes()
        {
            return Misc.MakeParamTypes(parameters);
        }

        ImmutableArray<string> IModuleItemInfo.GetTypeParams()
        {
            return default;
        }
    }
}