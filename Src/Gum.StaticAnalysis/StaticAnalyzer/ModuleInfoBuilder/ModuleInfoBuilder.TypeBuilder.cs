using Gum.CompileTime;
using System;
using System.Collections.Generic;

namespace Gum.StaticAnalysis
{
    public partial class ModuleInfoBuilder
    {
        class TypeBuilder
        {
            private TypeValue.Normal thisTypeValue;
            private Dictionary<Name, ModuleItemId> memberTypeIds;
            private Dictionary<Name, ModuleItemId> memberFuncIds;
            private Dictionary<Name, ModuleItemId> memberVarIds;

            public TypeBuilder(TypeValue.Normal thisTypeValue)
            {
                this.thisTypeValue = thisTypeValue;

                memberTypeIds = new Dictionary<Name, ModuleItemId>();
                memberFuncIds = new Dictionary<Name, ModuleItemId>();
                memberVarIds = new Dictionary<Name, ModuleItemId>();
            }

            public TypeValue.Normal GetThisTypeValue()
            {
                return thisTypeValue;
            }
        }
    }
}