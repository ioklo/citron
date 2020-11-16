using System;
using System.Collections.Generic;
using System.Text;

namespace Gum.CompileTime
{
    public class VarInfo : ItemInfo
    {   
        public bool bStatic { get; }
        public TypeValue TypeValue { get; }

        public VarInfo(ItemId id, bool bStatic, TypeValue typeValue)
            : base(id)
        {   
            this.bStatic = bStatic;
            TypeValue = typeValue;
        }
    }
}
