﻿using Gum.Core.IL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gum.App.Compiler
{
    class LocalInfo
    {
        public IType Type { get; private set; }
        public string Name { get; private set; }
        public int Index { get; private set; }

        public LocalInfo(IType type, string name, int index)
        {
            Type = type;
            Name = name;
            Index = index;
        }
    }
}
