﻿using Gum.Core.AbstractSyntax;
using Gum.Core.IL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gum.App.Compiler
{
    public class FieldInfo
    {
        public IType Type { get; private set; }
        public string Name { get; private set; }
        public AccessModifier Accessor { get; private set; }
        public int Index { get; private set; }

        public FieldInfo(AccessModifier a, IType type, string name, int i)
        {
            Type = type;
            Name = name;
            Accessor = a;
            Index = i;
        }
    }
}
