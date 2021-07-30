using Gum.Collections;
using Gum.Infra;
using Pretune;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using M = Gum.CompileTime;
using R = Gum.IR0;

namespace Gum.IR0Translator
{
    [ExcludeComparison]
    partial struct ExternalNamespaceDict
    {
        ImmutableDictionary<M.NamespaceName, ExternalModuleNamespaceInfo> namespaces;

        public ExternalNamespaceDict(ImmutableArray<M.NamespaceInfo> mnss)
        {
            var nssBuilder = ImmutableDictionary.CreateBuilder<M.NamespaceName, ExternalModuleNamespaceInfo>();
            foreach (var mns in mnss)
                nssBuilder.Add(mns.Name, new ExternalModuleNamespaceInfo(mns));
            namespaces = nssBuilder.ToImmutable();
        }

        public ExternalModuleNamespaceInfo? Get(M.NamespaceName name)
        {
            return namespaces.GetValueOrDefault(name);
        }
    }

    [ExcludeComparison]
    partial struct ModuleTypeDict
    {
        ImmutableDictionary<(M.Name Name, int TypeParamCount), IModuleTypeInfo> types;

        public ModuleTypeDict(IEnumerable<IModuleTypeInfo> types)
        {
            var typesBuilder = ImmutableDictionary.CreateBuilder<(M.Name Name, int TypeParamCount), IModuleTypeInfo>();
            foreach (var type in types)
            {
                typesBuilder.Add((type.GetName(), type.GetTypeParams().Length), type);
            }

            this.types = typesBuilder.ToImmutable();
        }

        public ModuleTypeDict(ImmutableArray<M.TypeInfo> mtypes)
        {
            var typesBuilder = ImmutableDictionary.CreateBuilder<(M.Name Name, int TypeParamCount), IModuleTypeInfo>();
            foreach (var mtype in mtypes)
            {
                var type = ExternalModuleMisc.Make(mtype);
                typesBuilder.Add((mtype.Name, mtype.TypeParams.Length), type);
            }
            types = typesBuilder.ToImmutable();
        }

        public IModuleTypeInfo? Get(M.Name name, int typeParamCount)
        {
            return types.GetValueOrDefault((name, typeParamCount));
        }
    }

    [ExcludeComparison]
    partial struct ModuleFuncDict
    {
        ImmutableDictionary<(M.Name Name, int TypeParamCount), ImmutableArray<IModuleFuncInfo>> funcs;
        ImmutableDictionary<(M.Name Name, int TypeParamCount, M.ParamTypes ParamTypes), IModuleFuncInfo> exactFuncs;

        public ModuleFuncDict(IEnumerable<IModuleFuncInfo> funcInfos)
        {
            var funcsDict = new Dictionary<(M.Name Name, int TypeParamCount), List<IModuleFuncInfo>>();
            var exactFuncsBuilder = ImmutableDictionary.CreateBuilder<(M.Name Name, int TypeParamCount, M.ParamTypes paramTypes), IModuleFuncInfo>();

            foreach (var func in funcInfos)
            {
                var funcName = func.GetName();
                var typeParamCount = func.GetTypeParams().Length;

                // mfunc의 typeparam이 n개면 n-1 .. 0 에도 다 넣는다
                for (int i = 0; i <= typeParamCount; i++)
                {
                    var key = (funcName, i);

                    if (!funcsDict.TryGetValue(key, out var list))
                    {
                        list = new List<IModuleFuncInfo>();
                        funcsDict.Add(key, list);
                    }

                    list.Add(func);
                }

                var paramTypes = func.GetParamTypes();
                exactFuncsBuilder.Add((funcName, typeParamCount, paramTypes), func);
            }

            // funcsDict제작이 다 끝났으면
            var builder = ImmutableDictionary.CreateBuilder<(M.Name Name, int TypeParamCount), ImmutableArray<IModuleFuncInfo>>();
            foreach (var keyValue in funcsDict)
                builder.Add(keyValue.Key, keyValue.Value.ToImmutableArray());
            this.funcs = builder.ToImmutable();

            exactFuncs = exactFuncsBuilder.ToImmutable();
        }

        public ModuleFuncDict(ImmutableArray<M.FuncInfo> mfuncs)
            : this(mfuncs.Select(mfunc => (IModuleFuncInfo)new ExternalModuleFuncInfo(mfunc)))
        {   
        }

        public ImmutableArray<IModuleFuncInfo> Get(M.Name name, int minTypeParamCount)
        {
            return funcs.GetValueOrDefault((name, minTypeParamCount));
        }

        public IModuleFuncInfo? Get(M.Name name, int typeParamCount, M.ParamTypes paramTypes)
        {
            return exactFuncs.GetValueOrDefault((name, typeParamCount, paramTypes));            
        }
    }

    [ImplementIEquatable]
    partial class ExternalModuleInfo : IModuleInfo
    {
        M.ModuleName name;
        ExternalNamespaceDict namespaceDict;
        ModuleTypeDict typeDict;
        ModuleFuncDict funcDict;

        void IPure.EnsurePure()
        {
            throw new NotImplementedException();
        }

        public ExternalModuleInfo(M.ModuleInfo moduleInfo)
        {
            name = moduleInfo.Name;

            namespaceDict = new ExternalNamespaceDict(moduleInfo.Namespaces);
            typeDict = new ModuleTypeDict(moduleInfo.Types);
            funcDict = new ModuleFuncDict(moduleInfo.Funcs);
        }

        public M.ModuleName GetName()
        {
            return name;
        }

        M.ModuleName IModuleInfo.GetName()
        {
            return name;
        }

        IModuleNamespaceInfo? IModuleNamespaceContainer.GetNamespace(M.NamespaceName name)
        {
            return namespaceDict.Get(name);
        }

        IModuleTypeInfo? IModuleTypeContainer.GetType(M.Name name, int typeParamCount)
        {
            return typeDict.Get(name, typeParamCount);
        }

        ImmutableArray<IModuleFuncInfo> IModuleFuncContainer.GetFuncs(M.Name name, int minTypeParamCount)
        {
            return funcDict.Get(name, minTypeParamCount);
        }

        IModuleFuncInfo? IModuleFuncContainer.GetFunc(M.Name name, int typeParamCount, M.ParamTypes paramTypes)
        {
            return funcDict.Get(name, typeParamCount, paramTypes);
        }        
    }

    [ImplementIEquatable]
    partial class ExternalModuleNamespaceInfo : IModuleNamespaceInfo
    {
        M.NamespaceName name;
        ExternalNamespaceDict namespaceDict;
        ModuleTypeDict typeDict;
        ModuleFuncDict funcDict;

        public ExternalModuleNamespaceInfo(M.NamespaceInfo mns)
        {
            name = mns.Name;
            namespaceDict = new ExternalNamespaceDict(mns.Namespaces);
            typeDict = new ModuleTypeDict(mns.Types);
            funcDict = new ModuleFuncDict(mns.Funcs);
        }

        M.NamespaceName IModuleNamespaceInfo.GetName()
        {
            return name;
        }

        IModuleNamespaceInfo? IModuleNamespaceContainer.GetNamespace(M.NamespaceName name)
        {
            return namespaceDict.Get(name);
        }

        IModuleTypeInfo? IModuleTypeContainer.GetType(M.Name name, int typeParamCount)
        {
            return typeDict.Get(name, typeParamCount);
        }

        ImmutableArray<IModuleFuncInfo> IModuleFuncContainer.GetFuncs(M.Name name, int minTypeParamCount)
        {
            return funcDict.Get(name, minTypeParamCount);
        }

        IModuleFuncInfo? IModuleFuncContainer.GetFunc(M.Name name, int typeParamCount, M.ParamTypes paramTypes)
        {
            return funcDict.Get(name, typeParamCount, paramTypes);
        }
    }
    
    [AutoConstructor, ImplementIEquatable]
    partial class ExternalModuleFuncInfo : IModuleFuncInfo
    {
        M.FuncInfo funcInfo;

        M.AccessModifier IModuleCallableInfo.GetAccessModifier()
        {
            return funcInfo.AccessModifier;
        }

        M.Name IModuleItemInfo.GetName()
        {
            return funcInfo.Name;
        }

        ImmutableArray<M.Param> IModuleCallableInfo.GetParameters()
        {
            return funcInfo.Parameters;
        }

        M.ParamTypes IModuleCallableInfo.GetParamTypes()
        {
            return Misc.MakeParamTypes(funcInfo.Parameters);
        }

        M.Type IModuleFuncInfo.GetReturnType()
        {
            return funcInfo.RetType;
        }
        
        ImmutableArray<string> IModuleItemInfo.GetTypeParams()
        {
            return funcInfo.TypeParams;
        }

        bool IModuleFuncInfo.IsInstanceFunc()
        {
            return funcInfo.IsInstanceFunc;
        }

        bool IModuleFuncInfo.IsSequenceFunc()
        {
            return funcInfo.IsSequenceFunc;
        }        
    }

    [AutoConstructor, ImplementIEquatable]
    partial class ExternalModuleConstructorInfo : IModuleConstructorInfo
    {
        M.ConstructorInfo constructorInfo;

        M.AccessModifier IModuleCallableInfo.GetAccessModifier()
        {
            return constructorInfo.AccessModifier;
        }

        M.Name IModuleItemInfo.GetName()
        {
            return constructorInfo.Name;
        }

        ImmutableArray<M.Param> IModuleCallableInfo.GetParameters()
        {
            return constructorInfo.Parameters;
        }

        M.ParamTypes IModuleCallableInfo.GetParamTypes()
        {
            return Misc.MakeParamTypes(constructorInfo.Parameters);
        }

        ImmutableArray<string> IModuleItemInfo.GetTypeParams()
        {
            return ImmutableArray<string>.Empty;
        }
    }

    [AutoConstructor, ImplementIEquatable]
    partial class ExternalModuleEnumInfo : IModuleEnumInfo
    {        
        M.EnumInfo enumInfo;

        IModuleEnumElemInfo? IModuleEnumInfo.GetElem(M.Name memberName)
        {
            foreach (var elemInfo in enumInfo.ElemInfos)
                if (elemInfo.Name.Equals(memberName))
                    return new ExternalModuleEnumElemInfo(elemInfo);

            return null;
        }

        IModuleFuncInfo? IModuleFuncContainer.GetFunc(M.Name name, int typeParamCount, M.ParamTypes paramTypes)
        {
            throw new NotImplementedException();
        }

        ImmutableArray<IModuleFuncInfo> IModuleFuncContainer.GetFuncs(M.Name name, int minTypeParamCount)
        {
            throw new NotImplementedException();
        }

        M.Name IModuleItemInfo.GetName()
        {
            throw new NotImplementedException();
        }

        IModuleTypeInfo? IModuleTypeContainer.GetType(M.Name name, int typeParamCount)
        {
            throw new NotImplementedException();
        }

        ImmutableArray<string> IModuleItemInfo.GetTypeParams()
        {
            throw new NotImplementedException();
        }
    }

    [AutoConstructor, ImplementIEquatable]
    partial class ExternalModuleMemberVarInfo : IModuleMemberVarInfo
    {   
        M.MemberVarInfo info;

        M.Name IModuleItemInfo.GetName()
        {
            return info.Name;
        }

        ImmutableArray<string> IModuleItemInfo.GetTypeParams()
        {
            return ImmutableArray<string>.Empty;
        }

        M.Type IModuleMemberVarInfo.GetDeclType()
        {
            return info.Type; 
        }

        bool IModuleMemberVarInfo.IsStatic()
        {
            return info.IsStatic;
        }

        M.AccessModifier IModuleMemberVarInfo.GetAccessModifier()
        {
            return info.AccessModifier;
        }
    }

    [AutoConstructor, ImplementIEquatable]
    partial class ExternalModuleEnumElemInfo : IModuleEnumElemInfo
    {
        M.EnumElemInfo elemInfo;

        ImmutableArray<IModuleMemberVarInfo> IModuleEnumElemInfo.GetFieldInfos()
        {
            var builder = ImmutableArray.CreateBuilder<IModuleMemberVarInfo>(elemInfo.FieldInfos.Length);

            foreach (var fieldInfo in elemInfo.FieldInfos)
                builder.Add(new ExternalModuleMemberVarInfo(fieldInfo));

            return builder.MoveToImmutable();
        }

        IModuleFuncInfo? IModuleFuncContainer.GetFunc(M.Name name, int typeParamCount, M.ParamTypes paramTypes)
        {
            throw new NotImplementedException();
        }

        ImmutableArray<IModuleFuncInfo> IModuleFuncContainer.GetFuncs(M.Name name, int minTypeParamCount)
        {
            throw new NotImplementedException();
        }

        M.Name IModuleItemInfo.GetName()
        {
            return elemInfo.Name;
        }

        IModuleTypeInfo? IModuleTypeContainer.GetType(M.Name name, int typeParamCount)
        {
            throw new NotImplementedException();
        }

        ImmutableArray<string> IModuleItemInfo.GetTypeParams()
        {
            throw new NotImplementedException();
        }

        bool IModuleEnumElemInfo.IsStandalone()
        {
            return elemInfo.FieldInfos.Length == 0;
        }
    }

    static class ExternalModuleMisc
    {
        public static IModuleTypeInfo Make(M.TypeInfo typeInfo)
        {
            switch(typeInfo)
            {
                case M.StructInfo structInfo:
                    return new ExternalModuleStructInfo(structInfo);

                case M.EnumInfo enumInfo:
                    return new ExternalModuleEnumInfo(enumInfo);

                case M.EnumElemInfo enumElemInfo:
                    return new ExternalModuleEnumElemInfo(enumElemInfo);

                default:
                    throw new UnreachableCodeException();
            }
        }
    }

    [ImplementIEquatable]
    partial class ExternalModuleStructInfo : IModuleStructInfo
    {
        M.StructInfo structInfo;
        ModuleTypeDict typeDict;

        public ExternalModuleStructInfo(M.StructInfo structInfo)
        {
            this.structInfo = structInfo;
            this.typeDict = new ModuleTypeDict(structInfo.MemberTypes);
        }

        IModuleFuncInfo? IModuleFuncContainer.GetFunc(M.Name name, int typeParamCount, M.ParamTypes paramTypes)
        {
            throw new NotImplementedException();
        }

        ImmutableArray<IModuleFuncInfo> IModuleFuncContainer.GetFuncs(M.Name name, int minTypeParamCount)
        {
            throw new NotImplementedException();
        }

        ImmutableArray<IModuleTypeInfo> IModuleStructInfo.GetMemberTypes()
        {
            var builder = ImmutableArray.CreateBuilder<IModuleTypeInfo>(structInfo.MemberTypes.Length);

            foreach (var memberType in structInfo.MemberTypes)
            {
                var t = ExternalModuleMisc.Make(memberType);
                builder.Add(t);
            }

            return builder.MoveToImmutable();
        }

        ImmutableArray<IModuleFuncInfo> IModuleStructInfo.GetMemberFuncs()
        {
            var builder = ImmutableArray.CreateBuilder<IModuleFuncInfo>(structInfo.MemberFuncs.Length);

            foreach(var memberFunc in structInfo.MemberFuncs)
            {
                var f = new ExternalModuleFuncInfo(memberFunc);
                builder.Add(f);
            }

            return builder.MoveToImmutable();
        }

        M.Name IModuleItemInfo.GetName()
        {
            return structInfo.Name;
        }

        IModuleTypeInfo? IModuleTypeContainer.GetType(M.Name name, int typeParamCount)
        {
            return typeDict.Get(name, typeParamCount);
        }

        ImmutableArray<string> IModuleItemInfo.GetTypeParams()
        {
            return structInfo.TypeParams;
        }

        ImmutableArray<IModuleConstructorInfo> IModuleStructInfo.GetConstructors()
        {
            var builder = ImmutableArray.CreateBuilder<IModuleConstructorInfo>(structInfo.Constructors.Length);

            foreach (var constructor in structInfo.Constructors)
                builder.Add(new ExternalModuleConstructorInfo(constructor));

            return builder.MoveToImmutable();
        }

        ImmutableArray<IModuleMemberVarInfo> IModuleStructInfo.GetMemberVars()
        {
            var builder = ImmutableArray.CreateBuilder<IModuleMemberVarInfo>(structInfo.MemberVars.Length);

            foreach (var memberVar in structInfo.MemberVars)
                builder.Add(new ExternalModuleMemberVarInfo(memberVar));

            return builder.MoveToImmutable();
        }

        M.Type? IModuleStructInfo.GetBaseType()
        {
            return structInfo.BaseType;
        }

        IModuleConstructorInfo? IModuleStructInfo.GetAutoConstructor()
        {
            return null; // External에서 얻어온 것은 Auto/아닌 것의 기준이 없다
        }
    }
}
