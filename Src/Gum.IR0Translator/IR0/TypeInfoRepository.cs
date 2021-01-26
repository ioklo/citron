using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Gum.CompileTime;

namespace Gum.IR0
{
    interface ITypeInfoRepository
    {
        public TypeInfo? GetType(ItemId id);
        public IEnumerable<TypeInfo> GetTypes(ItemPath path);
    }

    // Phase3에서 ItemInfo얻어오는 용도
    class TypeInfoRepository : ITypeInfoRepository
    {
        ModuleInfo internalModuleInfo;
        ModuleInfoRepository externalModuleInfoRepo;

        public TypeInfoRepository(ModuleInfo internalModuleInfo, ModuleInfoRepository moduleInfoRepo)
        {
            this.internalModuleInfo = internalModuleInfo;
            this.externalModuleInfoRepo = moduleInfoRepo;
        }

        TItemInfo? GetInternalItem<TItemInfo>(ItemPath path)
            where TItemInfo : ItemInfo
        {
            if (path.OuterEntries.Length == 0)
                return internalModuleInfo.GetGlobalItem(path.NamespacePath, path.Entry) as TItemInfo;

            var curTypeInfo = internalModuleInfo.GetGlobalItem(path.NamespacePath, path.OuterEntries[0]) as TypeInfo;
            if (curTypeInfo == null) return null;

            for (int i = 1; i < path.OuterEntries.Length; i++)
            {
                curTypeInfo = curTypeInfo.GetItem(path.OuterEntries[i]) as TypeInfo;
                if (curTypeInfo == null) return null;
            }

            return curTypeInfo.GetItem(path.Entry) as TItemInfo;
        }

        TItemInfo? GetExternalItem<TItemInfo>(ModuleInfo moduleInfo, ItemPath path)
            where TItemInfo : ItemInfo
        {
            if (path.OuterEntries.Length == 0)
                return moduleInfo.GetGlobalItem(path.NamespacePath, path.Entry) as TItemInfo;

            var curTypeInfo = moduleInfo.GetGlobalItem(path.NamespacePath, path.OuterEntries[0]) as TypeInfo;
            if (curTypeInfo == null) return null;

            for (int i = 1; i < path.OuterEntries.Length; i++)
            {
                curTypeInfo = curTypeInfo.GetItem(path.OuterEntries[i]) as TypeInfo;
                if (curTypeInfo == null) return null;
            }

            return curTypeInfo.GetItem(path.Entry) as TItemInfo;
        }

        // Id로 바로 찾기, 
        public TItemInfo? GetItem<TItemInfo>(ItemId id)
            where TItemInfo : ItemInfo
        {
            if (ModuleInfoEqualityComparer.EqualsModuleName(id.ModuleName, ModuleName.Internal))
                return GetInternalItem<TItemInfo>(id.GetItemPath());

            var moduleInfo = externalModuleInfoRepo.GetModule(id.ModuleName);
            if (moduleInfo == null) return null;

            return GetExternalItem<TItemInfo>(moduleInfo, id.GetItemPath());
        }

        TypeInfo? GetInternalType(NamespacePath nsPath, ImmutableArray<ItemPathEntry> entries)
        {
            var e = entries.GetEnumerator();
            if (!e.MoveNext()) return null;

            var curTypeInfo = internalModuleInfo.GetGlobalItem(nsPath, e.Current) as TypeInfo;
            if (curTypeInfo == null) return null;

            while (e.MoveNext())
            {
                curTypeInfo = curTypeInfo.GetItem(e.Current) as TypeInfo;
                if (curTypeInfo == null) return null;
            }

            return curTypeInfo;
        }

        TypeInfo? GetExternalType(ModuleInfo moduleInfo, NamespacePath nsPath, ImmutableArray<ItemPathEntry> entries)
        {
            var e = entries.GetEnumerator();
            if (!e.MoveNext()) return null;

            var curTypeInfo = moduleInfo.GetGlobalItem(nsPath, e.Current) as TypeInfo;
            if (curTypeInfo == null) return null;

            while (e.MoveNext())
            {
                curTypeInfo = curTypeInfo.GetItem(e.Current) as TypeInfo;
                if (curTypeInfo == null) return null;
            }

            return curTypeInfo;
        }

        // TODO: 이 함수 하나만 밖으로 내놓고, 나머지는 정리
        public IEnumerable<TypeInfo> GetTypes(ItemPath typePath)
        {
            return GetTypes(typePath.NamespacePath, typePath.OuterEntries, typePath.Entry);
        }

        public IEnumerable<TypeInfo> GetTypes(NamespacePath nsPath, ItemPathEntry entry)
        {
            return GetTypes(nsPath, Array.Empty<ItemPathEntry>(), entry);
        }

        // NS.MyType1<,>.MyType2<> 검색
        public IEnumerable<TypeInfo> GetTypes(NamespacePath nsPath, IEnumerable<ItemPathEntry> outerEntries, ItemPathEntry entry)
        {
            var fullEntries = outerEntries.Append(entry).ToImmutableArray();
            var internalTypeInfo = GetInternalType(nsPath, fullEntries);
            if (internalTypeInfo != null)
                yield return internalTypeInfo;

            foreach (var externalModuleInfo in externalModuleInfoRepo.GetAllModules())
            {
                var typeInfo = GetExternalType(externalModuleInfo, nsPath, fullEntries);
                if (typeInfo != null)
                    yield return typeInfo;
            }
        }

        // 이름만으로 함수들을 찾는 함수
        public IEnumerable<FuncInfo> GetFuncs(NamespacePath nsPath, Name funcName)
        {
            return GetFuncs(nsPath, ImmutableArray<ItemPathEntry>.Empty, funcName);
        }

        public IEnumerable<FuncInfo> GetFuncs(
            NamespacePath nsPath,
            ImmutableArray<ItemPathEntry> outerEntries,
            Name funcName)
        {
            if (outerEntries.Length == 0)
            {
                foreach (var internalFuncInfo in internalModuleInfo.GetGlobalFuncs(nsPath, funcName))
                    yield return internalFuncInfo;

                foreach (var moduleInfo in externalModuleInfoRepo.GetAllModules())
                {
                    // 이름만 갖고 Func를 얻어낸다
                    foreach (var funcInfo in moduleInfo.GetGlobalFuncs(nsPath, funcName))
                        yield return funcInfo;
                }
            }
            else
            {
                var internalTypeInfo = GetInternalType(nsPath, outerEntries);
                if (internalTypeInfo != null)
                {
                    var funcInfos = internalTypeInfo.GetFuncs(funcName);
                    foreach (var funcInfo in funcInfos)
                        yield return funcInfo;
                }

                foreach (var moduleInfo in externalModuleInfoRepo.GetAllModules())
                {
                    var typeInfo = GetExternalType(moduleInfo, nsPath, outerEntries);
                    if (typeInfo == null) continue;

                    var funcInfos = typeInfo.GetFuncs(funcName);
                    foreach (var funcInfo in funcInfos)
                        yield return funcInfo;
                }
            }
        }

        // 이름으로 변수를 찾는 함수

        public IEnumerable<VarInfo> GetVars(NamespacePath nsPath, Name varName)
        {
            return GetVars(nsPath, ImmutableArray<ItemPathEntry>.Empty, varName);
        }

        public IEnumerable<VarInfo> GetVars(
            NamespacePath nsPath,
            ImmutableArray<ItemPathEntry> outerEntries,
            Name varName)
        {
            if (outerEntries.Length == 0)
            {
                var internalVarInfo = internalModuleInfo.GetGlobalVar(nsPath, varName);
                if (internalVarInfo != null) 
                    yield return internalVarInfo;

                foreach (var moduleInfo in externalModuleInfoRepo.GetAllModules())
                {
                    var varInfo = moduleInfo.GetGlobalVar(nsPath, varName);
                    if (varInfo != null)
                        yield return varInfo;
                }
            }
            else
            {
                var internalTypeInfo = GetInternalType(nsPath, outerEntries);
                if (internalTypeInfo != null)
                {
                    var varInfo = internalTypeInfo.GetVar(varName);
                    if (varInfo != null)
                        yield return varInfo;
                }

                foreach (var moduleInfo in externalModuleInfoRepo.GetAllModules())
                {
                    var typeInfo = GetExternalType(moduleInfo, nsPath, outerEntries);
                    if (typeInfo == null) continue;

                    var varInfo = typeInfo.GetVar(varName);
                    if (varInfo != null)
                        yield return varInfo;
                }
            }
        }
    }
}
