using Gum.Infra;
using Pretune;
using System;
using System.Collections.Generic;
using Gum.Collections;
using System.Linq;

namespace Gum.IR0
{
    [AutoConstructor, ImplementIEquatable]
    public partial struct Func
    {
        public Path Path { get; }
    }
}