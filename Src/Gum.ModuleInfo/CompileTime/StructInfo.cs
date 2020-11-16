using System;
using System.Collections.Generic;
using System.Text;

namespace Gum.CompileTime
{
    public class StructInfo : TypeInfo
    {
        public StructInfo(
            ItemId id,
            IEnumerable<string> typeParams,
            TypeValue? baseTypeValue,
            IEnumerable<ItemInfo> memberInfos)
            : base(id, typeParams, baseTypeValue, memberInfos)
        {
        }
    }
}
