using System;
using System.Collections.Generic;
using System.Text;

namespace Gum.CompileTime
{
    public class StructInfo : DefaultTypeInfo, IStructInfo
    {
        public StructInfo(
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
