using Pretune;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
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
    public partial class EnumDecl : TypeDecl
    {
        public override TypeDeclId Id { get; }
        public string Name { get; }
        public ImmutableArray<string> TypeParams { get; }
        public ImmutableArray<EnumElement> Elems { get; }
    }    
}
