using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

using M = Gum.CompileTime;

namespace Gum.IR0
{
    interface ITypeInfoRepository
    {
        public M.TypeInfo? GetType(ItemId id);
        public IEnumerable<M.TypeInfo> GetTypes(ItemPath path);
    }

    // Phase3에서 타입을 얻어오는 용도
    class TypeInfoRepository : ITypeInfoRepository
    {
        M.ModuleInfo internalModuleInfo;
        ModuleInfoRepository externalModuleInfoRepo;

        public TypeInfoRepository(M.ModuleInfo internalModuleInfo, ModuleInfoRepository moduleInfoRepo)
        {
            this.internalModuleInfo = internalModuleInfo;
            this.externalModuleInfoRepo = moduleInfoRepo;
        }

        public M.TypeInfo? GetType(M.ModuleName moduleName, M.NamespacePath namespacePath, M.Name name, int typeParamCount)
        {
            var itemPathEntry = new ItemPathEntry(name, typeParamCount);

            if (internalModuleInfo.Name.Equals(moduleName))
                return GlobalItemQueryService.GetGlobalItem(internalModuleInfo, namespacePath, itemPathEntry) as M.TypeInfo;

            foreach (var module in externalModuleInfoRepo.GetAllModules())
                if (module.Name.Equals(moduleName))
                    return GlobalItemQueryService.GetGlobalItem(module, namespacePath, itemPathEntry) as M.TypeInfo;

            return null;
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

        TypeInfo? GetInternalType(M.NamespacePath nsPath, ImmutableArray<ItemPathEntry> entries)
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
        public IEnumerable<M.TypeInfo> GetTypes(ItemPath typePath)
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
    }
}
