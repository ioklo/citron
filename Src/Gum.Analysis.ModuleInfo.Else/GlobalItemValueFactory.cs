//using Gum.Infra;
//using System;
//using System.Collections.Generic;
//using Gum.Collections;
//using System.Diagnostics;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//using M = Gum.CompileTime;
//using Citron.Analysis;

//namespace Citron.Analysis
//{
//    // 이름만으로 Global을 찾고 싶을땐.. 인데.. 사실 이름 전체로 찾고 싶을 경우가 더 많을 것이다
//    // 더 로우레벨인데 이름만 봐서는 그런 느낌이 들지 않아서, 이름 전체로 찾는 것과 이거 중에 고민을 하게 된다
//    public class GlobalItemValueFactory : IPure
//    {
//        // 내부 모듈 정보가 필요하다
//        IModuleInfo internalInfo;
//        ImmutableArray<IModuleInfo> externalInfos;

//        public GlobalItemValueFactory(IModuleInfo internalInfo, ImmutableArray<IModuleInfo> externalInfos)
//        {            
//            this.internalInfo = internalInfo;
//            this.externalInfos = externalInfos;
//        }

//        struct QueryContext
//        {
//            GlobalItemValueFactory outer;
//            M.Name name;
//            int typeParamCount;

//            public QueryContext(GlobalItemValueFactory outer, M.Name name, int typeParamCount)
//            {
//                this.outer = outer;
//                this.name = name;
//                this.typeParamCount = typeParamCount;
//            }       
            
//            ItemQueryResult GetGlobalTypeCore(M.ModuleName moduleName, IModuleTypeContainer typeContainer)
//            {
//                var type = typeContainer.GetType(name, typeParamCount);
//                if (type != null)
//                    return new ItemQueryResult.Type(new RootItemValueOuter(moduleName), type);

//                return ItemQueryResult.NotFound.Instance;
//            }

//            ItemQueryResult GetGlobalTypeInModule(IModuleInfo moduleInfo)
//            {
//                if (namespacePath.IsRoot)
//                    return GetGlobalTypeCore(moduleInfo.GetName(), moduleInfo);

//                var namespaceInfo = moduleInfo.GetNamespace(namespacePath);
//                if (namespaceInfo == null)
//                    return ItemQueryResult.NotFound.Instance;

//                return GetGlobalTypeCore(moduleInfo.GetName(), namespaceInfo);
//            }

//            ItemQueryResult GetGlobalType()
//            {
//                var candidates = new Candidates<ItemQueryResult>();

//                var internalResult = GetGlobalTypeInModule(outer.internalInfo);
//                if (internalResult is ItemQueryResult.Error) return internalResult;
//                if (internalResult is ItemQueryResult.Valid) candidates.Add(internalResult);
//                // empty 무시

//                foreach (var externalInfo in outer.externalInfos)
//                {
//                    var externalResult = GetGlobalTypeInModule(externalInfo);
//                    if (externalResult is ItemQueryResult.Error) return externalResult;
//                    if (externalResult is ItemQueryResult.Valid) candidates.Add(externalResult);
//                    // empty는 무시
//                }

//                var result = candidates.GetSingle();
//                if (result != null) return result;
//                if (candidates.IsEmpty) return ItemQueryResult.NotFound.Instance;
//                if (candidates.HasMultiple) return ItemQueryResult.Error.MultipleCandidates.Instance;

//                throw new UnreachableCodeException();
//            }

//            ItemQueryResult GetGlobalFuncCore(M.ModuleName moduleName, IModuleFuncContainer funcContainer)
//            {
//                var funcInfos = funcContainer.GetFuncs(name, typeParamCount);
                
//                if (funcInfos.Length != 0)
//                    return new ItemQueryResult.Funcs(new RootItemValueOuter(moduleName, namespacePath), funcInfos, false);
                
//                return ItemQueryResult.NotFound.Instance;
//            }

//            ItemQueryResult GetGlobalFuncInModule(IModuleInfo moduleInfo)
//            {
//                if (namespacePath.IsRoot)
//                    return GetGlobalFuncCore(moduleInfo.GetName(), moduleInfo);

//                var namespaceInfo = moduleInfo.GetNamespace(namespacePath);
//                if (namespaceInfo == null)
//                    return ItemQueryResult.NotFound.Instance;

//                return GetGlobalFuncCore(moduleInfo.GetName(), namespaceInfo);
//            }

//            ItemQueryResult GetGlobalFunc()
//            {
//                var candidates = new Candidates<ItemQueryResult>();

//                var internalResult = GetGlobalFuncInModule(outer.internalInfo);
//                if (internalResult is ItemQueryResult.Error) return internalResult;
//                if (internalResult is ItemQueryResult.Valid) candidates.Add(internalResult);
//                // empty 무시

//                foreach (var externalInfo in outer.externalInfos)
//                {
//                    var externalResult = GetGlobalFuncInModule(externalInfo);
//                    if (externalResult is ItemQueryResult.Error) return externalResult;
//                    if (externalResult is ItemQueryResult.Valid) candidates.Add(externalResult);
//                    // empty는 무시
//                }

//                var result = candidates.GetSingle();
//                if (result != null) return result;
//                if (candidates.IsEmpty) return ItemQueryResult.NotFound.Instance;
//                if (candidates.HasMultiple) return ItemQueryResult.Error.MultipleCandidates.Instance;

//                throw new UnreachableCodeException();
//            }

//            public ItemQueryResult GetGlobal()
//            {
//                var candidates = new Candidates<ItemQueryResult>();

//                // 타입끼리, 함수끼리 module을 건너서 중복체크를 한 뒤, 타입과 함수 결과를 가지고 중복체크를 한다
//                var typeResult = GetGlobalType();
//                if (typeResult is ItemQueryResult.Error) return typeResult;
//                if (typeResult is ItemQueryResult.Valid) candidates.Add(typeResult);
//                // empty 무시

//                var funcResult = GetGlobalFunc();
//                if (funcResult is ItemQueryResult.Error) return funcResult;
//                if (funcResult is ItemQueryResult.Valid) candidates.Add(funcResult);
//                // empty 무시

//                var result = candidates.GetSingle();
//                if (result != null) return result;
//                if (candidates.IsEmpty) return ItemQueryResult.NotFound.Instance;
//                if (candidates.HasMultiple) return ItemQueryResult.Error.MultipleCandidates.Instance;

//                throw new UnreachableCodeException();
//            }
//        }
        
//        public ItemQueryResult GetGlobal(M.NamespacePath namespacePath, M.Name name, int typeParamCount)
//        {
//            var context = new QueryContext(this, namespacePath, name, typeParamCount);
//            return context.GetGlobal();
//        }
//    }
//}
