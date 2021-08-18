using Pretune;
using System;
using System.Collections.Generic;
using Gum.Collections;
using System.Linq;
using System.Text;
using Gum.Infra;

namespace Gum.IR0
{ 
    [AutoConstructor, ImplementIEquatable]
    public partial struct EnumElementField : IPure
    {
        public Path Type { get; }
        public string Name { get; }

        public void EnsurePure()
        {
            throw new NotImplementedException();
        }
    }
    
    public record EnumElement(string Name, ImmutableArray<EnumElementField> Fields) : IPure
    {
        public void EnsurePure()
        {
            Misc.EnsurePure(Fields);
        }
    }
    
    public record EnumDecl(string Name, ImmutableArray<string> TypeParams, ImmutableArray<EnumElement> Elems) : TypeDecl
    {
        
    }    
}
