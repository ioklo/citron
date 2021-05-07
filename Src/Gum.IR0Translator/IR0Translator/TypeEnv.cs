using System;
using System.Collections.Generic;
using Gum.Collections;
using System.Diagnostics;

using M = Gum.CompileTime;

namespace Gum.IR0Translator
{       
    class TypeEnv
    {   
        ImmutableArray<TypeValue> data;
        
        public TypeEnv(ImmutableArray<TypeValue> data)
        {
            this.data = data;
        }

        public TypeValue GetValue(int index)
        {
            return data[index];
        }
    }
}