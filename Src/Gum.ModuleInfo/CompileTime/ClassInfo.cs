using System;
using System.Collections.Generic;
using System.Text;

namespace Gum.CompileTime
{
    public class ClassInfo : TypeInfo
    {
        public ClassInfo(
            ItemId id,
            IEnumerable<string> typeParams,
            TypeValue? baseTypeValue,
            IEnumerable<ItemInfo> items)
            : base(id, typeParams, baseTypeValue, items)
        {
        }
    }
}
