using Gum.Collections;
using Gum.Infra;
using System.Collections.Generic;
using System.Diagnostics;
using M = Gum.CompileTime;

namespace Gum.IR0Translator
{
    interface IModuleNamespaceContainer
    {
        IModuleNamespaceInfo? GetNamespace(M.NamespaceName name);
    }

    interface IModuleTypeContainer
    {
        IModuleTypeInfo? GetType(M.Name name, int typeParamCount); // type.Name.Equals(name) && type.TypeParams.Length == typeParamCount)
    }

    interface IModuleFuncContainer
    {
        IModuleFuncInfo? GetFunc(M.Name name, int typeParamCount, M.ParamTypes paramTypes);  // find exact one
        ImmutableArray<IModuleFuncInfo> GetFuncs(M.Name name, int minTypeParamCount); // find all possibilities
    }

    // ModuleInfo를 검색할때 쓰는 인터페이스, internal, external둘다 사용한다
    // M.ModuleInfo 대체
    interface IModuleInfo : IModuleNamespaceContainer, IModuleTypeContainer, IModuleFuncContainer, IPure
    {
        M.ModuleName GetName();
    }

    // M.NamespaceInfo대체
    interface IModuleNamespaceInfo : IModuleNamespaceContainer, IModuleTypeContainer, IModuleFuncContainer
    {
        M.NamespaceName GetName();
    }

    interface IModuleItemInfo 
    {
        M.Name GetName();
        ImmutableArray<string> GetTypeParams();
    }

    // M.TypeInfo 대체
    interface IModuleTypeInfo : IModuleItemInfo, IModuleTypeContainer, IModuleFuncContainer
    {
    }

    interface IModuleStructInfo : IModuleTypeInfo
    {
        IModuleConstructorInfo? GetAutoConstructor();
        ImmutableArray<IModuleTypeInfo> GetMemberTypes();
        ImmutableArray<IModuleFuncInfo> GetMemberFuncs();
        ImmutableArray<IModuleConstructorInfo> GetConstructors();
        ImmutableArray<IModuleMemberVarInfo> GetMemberVars();
        M.Type? GetBaseType();
    }

    interface IModuleClassInfo : IModuleTypeInfo
    {
        IModuleConstructorInfo? GetAutoConstructor();
        ImmutableArray<IModuleTypeInfo> GetMemberTypes();
        ImmutableArray<IModuleFuncInfo> GetMemberFuncs();
        ImmutableArray<IModuleConstructorInfo> GetConstructors();
        ImmutableArray<IModuleMemberVarInfo> GetMemberVars();
        M.Type? GetBaseType();
    }

    interface IModuleEnumInfo : IModuleTypeInfo
    {
        IModuleEnumElemInfo? GetElem(M.Name memberName);
    }

    interface IModuleCallableInfo : IModuleItemInfo
    {
        M.AccessModifier GetAccessModifier();
        ImmutableArray<M.Param> GetParameters();
        M.ParamTypes GetParamTypes();
    }

    // M.FuncInfo 대체
    interface IModuleFuncInfo : IModuleCallableInfo
    {
        bool IsInstanceFunc();
        bool IsSequenceFunc();
        M.Type GetReturnType();
        bool IsInternal();
    }

    // M.ConstructorInfo 대체
    interface IModuleConstructorInfo : IModuleCallableInfo
    {
    }

    // M.MemberVarInfo 대체
    interface IModuleMemberVarInfo : IModuleItemInfo
    {
        M.AccessModifier GetAccessModifier();
        M.Type GetDeclType();
        bool IsStatic();
    }

    // M.EnumElem 대체
    interface IModuleEnumElemInfo : IModuleTypeInfo
    {
        bool IsStandalone();
        ImmutableArray<IModuleMemberVarInfo> GetFieldInfos();
    }

    static class ModuleInfoExtensions
    {
        public static IModuleNamespaceInfo? GetNamespace(this IModuleNamespaceContainer namespaceContainer, M.NamespacePath namespacePath)
        {
            Debug.Assert(!namespacePath.IsRoot);

            IModuleNamespaceInfo? curNamespace = null;
            foreach (var entry in namespacePath.Entries)
            {
                curNamespace = namespaceContainer.GetNamespace(entry);

                if (curNamespace == null) return null;
                namespaceContainer = curNamespace;
            }

            return curNamespace;
        }
    }
}