using Gum.CompileTime;
using System;
using System.Collections.Generic;
using System.Text;

namespace Gum.IR0
{
    // 진짜 ItemInfoRepository 
    class ItemInfoRepository
    {
        public ItemInfoRepository()
        {
            
        }        

        // path로 찾기 
        public IEnumerable<IdentifierInfo> GetItems(ItemPath path, IInternalGlobalVariableRepository internalGlobalVarRepo)
        {
            if (path.NamespacePath.IsRoot && path.OuterEntries.IsEmpty)
            {
                if (path.Entry.TypeParamCount == 0 && path.Entry.ParamHash.Length == 0)
                {

                    // global variable에서 검색
                    var internalGlobalVarInfo = internalGlobalVarRepo.GetVariable(path.Entry.Name);
                    if (internalGlobalVarInfo != null) yield return internalGlobalVarInfo;
                }
            }

            throw new NotImplementedException();
        }
    }
}
