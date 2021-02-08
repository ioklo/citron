using Gum.Misc;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using M = Gum.CompileTime;

namespace Gum.IR0
{
    // 이름만으로 뭔가를 찾고 싶을땐
    class GlobalItemValueFactory
    {
        TypeValueFactory typeValueFactory;
        M.ModuleInfo internalModuleInfo;
        ModuleInfoRepository externalModuleRepos;

        public GlobalItemValueFactory(TypeValueFactory typeValueFactory, M.ModuleInfo internalModuleInfo, ModuleInfoRepository externalModuleRepos)
        {
            this.typeValueFactory = typeValueFactory;
            this.internalModuleInfo = internalModuleInfo;
            this.externalModuleRepos = externalModuleRepos;
        }
        
        struct QueryContext
        {
            GlobalItemValueFactory outer;
            M.NamespacePath namespacePath;
            M.Name name;
            ImmutableArray<TypeValue> typeArgs;
            TypeValue? hintType;

            public QueryContext(GlobalItemValueFactory outer, M.NamespacePath namespacePath, M.Name name, ImmutableArray<TypeValue> typeArgs, TypeValue? hintType)
            {
                this.outer = outer;
                this.namespacePath = namespacePath;
                this.name = name;
                this.typeArgs = typeArgs;
                this.hintType = hintType;                
            }       
            
            ItemResult GetGlobalTypeCore(M.ModuleName moduleName, ImmutableArray<M.TypeInfo> types)
            {
                var candidates = new Candidates<ItemResult>();

                foreach(var type in types)
                {
                    // typeHint는 고려하지 않는다
                    if (type.Name.Equals(name) && type.TypeParams.Length == typeArgs.Length)
                    {
                        var typeValue = outer.typeValueFactory.MakeGlobalType(moduleName, namespacePath, type, typeArgs);
                        candidates.Add(new ValueItemResult(typeValue));
                    }
                }

                var result = candidates.GetSingle();
                if (result != null) return result;
                if (candidates.IsEmpty) return NotFoundItemResult.Instance;
                if (candidates.HasMultiple) return MultipleCandidatesItemResult.Instance;

                throw new UnreachableCodeException();
            }

            ItemResult GetGlobalTypeInModule(M.ModuleInfo moduleInfo)
            {
                if (namespacePath.IsRoot)
                    return GetGlobalTypeCore(moduleInfo.Name, moduleInfo.Types);

                var namespaceInfo = moduleInfo.GetNamespace(namespacePath);
                if (namespaceInfo == null)
                    return NotFoundItemResult.Instance;

                return GetGlobalTypeCore(moduleInfo.Name, namespaceInfo.Types);
            }

            ItemResult GetGlobalType()
            {
                var candidates = new Candidates<ItemResult>();

                var internalResult = GetGlobalTypeInModule(outer.internalModuleInfo);
                if (internalResult is MultipleCandidatesItemResult) return internalResult;
                if (internalResult is ValueItemResult) candidates.Add(internalResult);
                // empty 무시

                foreach (var externalModuleInfo in outer.externalModuleRepos.GetAllModules())
                {
                    var externalResult = GetGlobalTypeInModule(externalModuleInfo);
                    if (externalResult is MultipleCandidatesItemResult) return externalResult;
                    if (externalResult is ValueItemResult) candidates.Add(externalResult);
                    // empty는 무시
                }

                var result = candidates.GetSingle();
                if (result != null) return result;
                if (candidates.IsEmpty) return NotFoundItemResult.Instance;
                if (candidates.HasMultiple) return MultipleCandidatesItemResult.Instance;

                throw new UnreachableCodeException();
            }

            ItemResult GetGlobalFuncCore(M.ModuleName moduleName, ImmutableArray<M.FuncInfo> funcs)
            {
                var candidates = new Candidates<ItemResult>();

                foreach (var func in funcs)
                {
                    if (func.Name.Equals(name) &&
                        func.TypeParams.Length == typeArgs.Length) // TODO: TypeParam inference, 같지 않아도 된다
                    {
                        var funcValue = outer.typeValueFactory.MakeGlobalFunc(moduleName, namespacePath, func, typeArgs);
                        candidates.Add(new ValueItemResult(funcValue));
                    }
                }

                var result = candidates.GetSingle();
                if (result != null) return result;
                if (candidates.IsEmpty) return NotFoundItemResult.Instance;
                if (candidates.HasMultiple) return MultipleCandidatesItemResult.Instance;

                throw new UnreachableCodeException();
            }

            ItemResult GetGlobalFuncInModule(M.ModuleInfo moduleInfo)
            {
                if (namespacePath.IsRoot)
                    return GetGlobalFuncCore(moduleInfo.Name, moduleInfo.Funcs);

                var namespaceInfo = moduleInfo.GetNamespace(namespacePath);
                if (namespaceInfo == null)
                    return NotFoundItemResult.Instance;

                return GetGlobalFuncCore(moduleInfo.Name, namespaceInfo.Funcs);
            }

            ItemResult GetGlobalFunc()
            {
                var candidates = new Candidates<ItemResult>();

                var internalResult = GetGlobalFuncInModule(outer.internalModuleInfo);
                if (internalResult is MultipleCandidatesItemResult) return internalResult;
                if (internalResult is ValueItemResult) candidates.Add(internalResult);
                // empty 무시

                foreach (var externalModuleInfo in outer.externalModuleRepos.GetAllModules())
                {
                    var externalResult = GetGlobalFuncInModule(externalModuleInfo);
                    if (externalResult is MultipleCandidatesItemResult) return externalResult;
                    if (externalResult is ValueItemResult) candidates.Add(externalResult);
                    // empty는 무시
                }

                var result = candidates.GetSingle();
                if (result != null) return result;
                if (candidates.IsEmpty) return NotFoundItemResult.Instance;
                if (candidates.HasMultiple) return MultipleCandidatesItemResult.Instance;

                throw new UnreachableCodeException();
            }

            public ItemResult GetGlobal()
            {
                var candidates = new Candidates<ItemResult>();

                // 타입끼리, 함수끼리 module을 건너서 중복체크를 한 뒤, 타입과 함수 결과를 가지고 중복체크를 한다
                var typeResult = GetGlobalType();
                if (typeResult is MultipleCandidatesItemResult) return typeResult;
                if (typeResult is ValueItemResult) candidates.Add(typeResult);
                // empty 무시

                var funcResult = GetGlobalFunc();
                if (funcResult is MultipleCandidatesItemResult) return funcResult;
                if (funcResult is ValueItemResult) candidates.Add(funcResult);
                // empty 무시

                var result = candidates.GetSingle();
                if (result != null) return result;
                if (candidates.IsEmpty) return NotFoundItemResult.Instance;
                if (candidates.HasMultiple) return MultipleCandidatesItemResult.Instance;

                throw new UnreachableCodeException();
            }
        }
        
        public ItemResult GetGlobal(M.NamespacePath namespacePath, M.Name name, ImmutableArray<TypeValue> typeArgs, TypeValue? hintTypeValue)
        {
            var context = new QueryContext(this, namespacePath, name, typeArgs, hintTypeValue);
            return context.GetGlobal();
        }
    }
}
