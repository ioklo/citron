using System;
using System.Collections.Generic;
using Gum.Collections;
using System.Linq;

using M = Gum.CompileTime;
using System.Diagnostics;
using Gum.Infra;
using Citron.Analysis;

namespace Citron.Analysis
{
    // reference module에서 타입 정보를 얻어오는 역할
    public class TypeInfoRepository
    {
        IModuleDecl internalInfo;
        ImmutableArray<IModuleDecl> externalInfos;

        public TypeInfoRepository(IModuleDecl internalInfo, ImmutableArray<IModuleDecl> externalInfos)
        {
            this.internalInfo = internalInfo;
            this.externalInfos = externalInfos;
        }

        IModuleDecl? GetModule(M.Name moduleName)
        {
            if (internalInfo.GetEntry().Name.Equals(moduleName))
                return internalInfo;

            foreach (var externalInfo in externalInfos)
                if (externalInfo.GetEntry().Name.Equals(moduleName))
                    return externalInfo;

            return null;
        }
        
        IModuleItemDecl? GetItem(M.DeclSymbolPath path)
        {
            if (path.Outer == null)
                return GetModule(path.Name);

            var outerItem = GetItem(path.Outer);
            if (outerItem == null) return null;

            return outerItem.GetItem(path.Name, path.TypeParamCount, path.ParamTypes);
        }

        public ITypeDeclSymbol? GetType(M.DeclSymbolPath path)
        {
            return GetItem(path) as ITypeDeclSymbol;
        }
    }
}
