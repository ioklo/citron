using Pretune;
using System;
using System.Collections.Generic;
using Gum.Collections;
using System.Linq;
using System.Text;

namespace Gum.IR0
{
    [AutoConstructor, ImplementIEquatable]
    public partial class EnumElement
    {
        public string Name { get; }
        public ImmutableArray<TypeAndName> Params { get; }
    }

    [AutoConstructor, ImplementIEquatable]
    public partial class EnumDecl : IDecl
    {   
        public string Name { get; }
        public ImmutableArray<string> TypeParams { get; }
        public ImmutableArray<EnumElement> Elems { get; }
    }    
}
