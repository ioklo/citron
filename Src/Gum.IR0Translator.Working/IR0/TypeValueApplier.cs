using Gum.CompileTime;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;

namespace Gum.IR0
{
    class TypeValueApplier
    {
        ITypeInfoRepository typeInfoRepo;

        public TypeValueApplier(ITypeInfoRepository typeInfoRepo)
        {
            this.typeInfoRepo = typeInfoRepo;
        }

        // ApplyTypeEnv_Normal(Normal (Z, [[T], [U], []]), { T -> int, U -> short })
        // 
        // Normal(Z, [[int], [short], []])
        
        public TypeValue Apply(TypeValue? context, TypeValue typeValue)
        {
            if (context is NormalTypeValue context_normal)
                return Apply_Normal(context_normal, typeValue);

            return typeValue;
        }
    }
}