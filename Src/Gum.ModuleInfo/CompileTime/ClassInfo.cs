using System;
using System.Collections.Generic;
using System.Text;

namespace Gum.CompileTime
{
    public class ClassInfo : DefaultTypeInfo, IClassInfo
    {
        public ClassInfo(
            ModuleItemId? outerTypeId,
            ModuleItemId typeId,
            IEnumerable<string> typeParams,
            TypeValue? baseTypeValue,
            IEnumerable<ModuleItemId> memberTypeIds,
            IEnumerable<ModuleItemId> memberFuncIds,
            IEnumerable<ModuleItemId> memberVarIds)
            : base(outerTypeId, typeId, typeParams, baseTypeValue, memberTypeIds, memberFuncIds, memberVarIds)
        {
        }
    }
}
