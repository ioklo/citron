using Gum.Infra;
using System;
using System.Collections.Generic;
using Gum.Collections;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using M = Gum.CompileTime;

namespace Gum.IR0Translator
{
    // 이름만으로 뭔가를 찾고 싶을땐
    class GlobalItemValueFactory : IPure
    {
        InternalModuleInfo internalModuleInfo;
        ModuleInfoRepository externalModuleRepos;

        public GlobalItemValueFactory(InternalModuleInfo internalModuleInfo, ModuleInfoRepository externalModuleRepos)
        {            
            this.internalModuleInfo = internalModuleInfo;
            this.externalModuleRepos = externalModuleRepos;
        }

        public void EnsurePure()
        {
            Infra.Misc.EnsurePure(internalModuleInfo);
            Infra.Misc.EnsurePure(externalModuleRepos);
        }

        struct QueryContext
        {
            GlobalItemValueFactory outer;
            M.NamespacePath namespacePath;
            M.Name name;
            int typeParamCount;

            public QueryContext(GlobalItemValueFactory outer, M.NamespacePath namespacePath, M.Name name, int typeParamCount)
            {
                this.outer = outer;
                this.namespacePath = namespacePath;
                this.name = name;
                this.typeParamCount = typeParamCount;
            }       
            
            ItemQueryResult GetGlobalTypeCore(M.ModuleName moduleName, IModuleTypeContainer typeContainer)
            {
                var type = typeContainer.GetType(name, typeParamCount);
                if (type != null)
                    return new ItemQueryResult.Type(new RootItemValueOuter(moduleName, namespacePath), type);

                return ItemQueryResult.NotFound.Instance;
            }

            ItemQueryResult GetGlobalTypeInModule(IModuleInfo moduleInfo)
            {
                if (namespacePath.IsRoot)
                    return GetGlobalTypeCore(moduleInfo.GetName(), moduleInfo);

                var namespaceInfo = moduleInfo.GetNamespace(namespacePath);
                if (namespaceInfo == null)
                    return ItemQueryResult.NotFound.Instance;

                return GetGlobalTypeCore(moduleInfo.GetName(), namespaceInfo);
            }

            ItemQueryResult GetGlobalType()
            {
                var candidates = new Candidates<ItemQueryResult>();

                var internalResult = GetGlobalTypeInModule(outer.internalModuleInfo);
                if (internalResult is ItemQueryResult.Error) return internalResult;
                if (internalResult is ItemQueryResult.Valid) candidates.Add(internalResult);
                // empty 무시

                foreach (var externalModuleInfo in outer.externalModuleRepos.GetAllModules())
                {
                    var externalResult = GetGlobalTypeInModule(externalModuleInfo);
                    if (externalResult is ItemQueryResult.Error) return externalResult;
                    if (externalResult is ItemQueryResult.Valid) candidates.Add(externalResult);
                    // empty는 무시
                }

                var result = candidates.GetSingle();
                if (result != null) return result;
                if (candidates.IsEmpty) return ItemQueryResult.NotFound.Instance;
                if (candidates.HasMultiple) return ItemQueryResult.Error.MultipleCandidates.Instance;

                throw new UnreachableCodeException();
            }

            ItemQueryResult GetGlobalFuncCore(M.ModuleName moduleName, IModuleFuncContainer funcContainer)
            {
                var funcInfos = funcContainer.GetFuncs(name, typeParamCount);
                
                if (funcInfos.Length != 0)
                    return new ItemQueryResult.Funcs(new RootItemValueOuter(moduleName, namespacePath), funcInfos, false);
                
                return ItemQueryResult.NotFound.Instance;
            }

            ItemQueryResult GetGlobalFuncInModule(IModuleInfo moduleInfo)
            {
                if (namespacePath.IsRoot)
                    return GetGlobalFuncCore(moduleInfo.GetName(), moduleInfo);

                var namespaceInfo = moduleInfo.GetNamespace(namespacePath);
                if (namespaceInfo == null)
                    return ItemQueryResult.NotFound.Instance;

                return GetGlobalFuncCore(moduleInfo.GetName(), namespaceInfo);
            }

            ItemQueryResult GetGlobalFunc()
            {
                var candidates = new Candidates<ItemQueryResult>();

                var internalResult = GetGlobalFuncInModule(outer.internalModuleInfo);
                if (internalResult is ItemQueryResult.Error) return internalResult;
                if (internalResult is ItemQueryResult.Valid) candidates.Add(internalResult);
                // empty 무시

                foreach (var externalModuleInfo in outer.externalModuleRepos.GetAllModules())
                {
                    var externalResult = GetGlobalFuncInModule(externalModuleInfo);
                    if (externalResult is ItemQueryResult.Error) return externalResult;
                    if (externalResult is ItemQueryResult.Valid) candidates.Add(externalResult);
                    // empty는 무시
                }

                var result = candidates.GetSingle();
                if (result != null) return result;
                if (candidates.IsEmpty) return ItemQueryResult.NotFound.Instance;
                if (candidates.HasMultiple) return ItemQueryResult.Error.MultipleCandidates.Instance;

                throw new UnreachableCodeException();
            }

            public ItemQueryResult GetGlobal()
            {
                var candidates = new Candidates<ItemQueryResult>();

                // 타입끼리, 함수끼리 module을 건너서 중복체크를 한 뒤, 타입과 함수 결과를 가지고 중복체크를 한다
                var typeResult = GetGlobalType();
                if (typeResult is ItemQueryResult.Error) return typeResult;
                if (typeResult is ItemQueryResult.Valid) candidates.Add(typeResult);
                // empty 무시

                var funcResult = GetGlobalFunc();
                if (funcResult is ItemQueryResult.Error) return funcResult;
                if (funcResult is ItemQueryResult.Valid) candidates.Add(funcResult);
                // empty 무시

                var result = candidates.GetSingle();
                if (result != null) return result;
                if (candidates.IsEmpty) return ItemQueryResult.NotFound.Instance;
                if (candidates.HasMultiple) return ItemQueryResult.Error.MultipleCandidates.Instance;

                throw new UnreachableCodeException();
            }
        }
        
        public ItemQueryResult GetGlobal(M.NamespacePath namespacePath, M.Name name, int typeParamCount)
        {
            var context = new QueryContext(this, namespacePath, name, typeParamCount);
            return context.GetGlobal();
        }
    }
}
