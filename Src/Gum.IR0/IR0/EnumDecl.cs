using Pretune;
using System;
using System.Collections.Generic;
using Gum.Collections;
using System.Linq;
using System.Text;
using Gum.Infra;

namespace Citron.IR0
{ 
    [AutoConstructor, ImplementIEquatable]
    public partial struct EnumElementField
    {
        public Path Type { get; }
        public string Name { get; }
    }
    
    public record EnumElement(string Name, ImmutableArray<EnumElementField> Fields)
    {
    }
    
    public record EnumDecl(string Name, ImmutableArray<string> TypeParams, ImmutableArray<EnumElement> Elems) : TypeDecl
    {
        
    }    
}
