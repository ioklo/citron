using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Gum.CompileTime;

namespace Gum.IR0
{
    class ItemInfoRepository
    {
        ModuleInfoRepository moduleInfoRepo;

        public ItemInfoRepository(ModuleInfoRepository moduleInfoRepo)
        {
            this.moduleInfoRepo = moduleInfoRepo;
        }

        // Id로 바로 찾기
        public TItemInfo? GetItem<TItemInfo>(ItemId id)
            where TItemInfo : ItemInfo
        {
            var moduleInfo = moduleInfoRepo.GetModule(id.ModuleName);
            if (moduleInfo == null) return null;

            if (id.OuterEntries.Length == 0)
                return moduleInfo.GetItem(id.NamespacePath, id.Entry) as TItemInfo;

            var curTypeInfo = moduleInfo.GetItem(id.NamespacePath, id.OuterEntries[0]) as TypeInfo;
            if (curTypeInfo == null) return null;

            for (int i = 1; i < id.OuterEntries.Length; i++)
            {
                curTypeInfo = curTypeInfo.GetItem(id.OuterEntries[i]) as TypeInfo;
                if (curTypeInfo == null) return null;
            }

            return curTypeInfo.GetItem(id.Entry) as TItemInfo;
        }

        TypeInfo? GetType(IModuleInfo moduleInfo, NamespacePath nsPath, IEnumerable<ItemPathEntry> entries)
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
            foreach (var moduleInfo in moduleInfoRepo.GetAllModules())
            {
                var typeInfo = GetType(moduleInfo, nsPath, outerEntries.Append(entry));
                if (typeInfo != null)
                    yield return typeInfo;
            }
        }

        // 이름만으로 함수들을 찾는 함수
        public IEnumerable<FuncInfo> GetFuncs(NamespacePath nsPath, Name funcName)
        {
            return GetFuncs(nsPath, Array.Empty<ItemPathEntry>(), funcName);
        }

        public IEnumerable<FuncInfo> GetFuncs(
            NamespacePath nsPath,
            IEnumerable<ItemPathEntry> outerEntries,
            Name funcName)
        {
            if (outerEntries.Any())
            {
                foreach (var moduleInfo in moduleInfoRepo.GetAllModules())
                {
                    // 이름만 갖고 Func를 얻어낸다
                    foreach (var funcInfo in moduleInfo.GetGlobalFuncs(nsPath, funcName))
                        yield return funcInfo;
                }
            }
            else
            {
                foreach (var moduleInfo in moduleInfoRepo.GetAllModules())
                {
                    var typeInfo = GetType(moduleInfo, nsPath, outerEntries);
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
            return GetVars(nsPath, Array.Empty<ItemPathEntry>(), varName);
        }

        public IEnumerable<VarInfo> GetVars(
            NamespacePath nsPath,
            IEnumerable<ItemPathEntry> outerEntries,
            Name varName)
        {
            if (outerEntries.Any())
            {
                foreach (var moduleInfo in moduleInfoRepo.GetAllModules())
                {
                    var varInfo = moduleInfo.GetGlobalVar(nsPath, varName);
                    if (varInfo != null)
                        yield return varInfo;
                }
            }
            else
            {
                foreach (var moduleInfo in moduleInfoRepo.GetAllModules())
                {
                    var typeInfo = GetType(moduleInfo, nsPath, outerEntries);
                    if (typeInfo == null) continue;

                    var varInfo = typeInfo.GetVar(varName);
                    if (varInfo != null)
                        yield return varInfo;
                }
            }
        }
    }
}
