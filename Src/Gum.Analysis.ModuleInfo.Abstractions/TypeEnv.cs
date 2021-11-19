﻿using System;
using System.Collections.Generic;
using Gum.Collections;
using System.Diagnostics;

using M = Gum.CompileTime;

namespace Gum.Analysis
{       
    public class TypeEnv
    {
        public static readonly TypeEnv None = new TypeEnv(default);
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