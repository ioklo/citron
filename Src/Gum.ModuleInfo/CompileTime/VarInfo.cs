using System;
using System.Collections.Generic;
using System.Text;

namespace Gum.CompileTime
{
    public class VarInfo
    {   
        public ModuleItemId? OuterId { get; }
        public ModuleItemId VarId { get; }
        public bool bStatic { get; }
        public TypeValue TypeValue { get; }

        public VarInfo(ModuleItemId? outerId, ModuleItemId varId, bool bStatic, TypeValue typeValue)
        {
            OuterId = outerId;
            VarId = varId;
            this.bStatic = bStatic;
            TypeValue = typeValue;
        }
    }
}
