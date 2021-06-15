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

    [AutoConstructor, ImplementIEquatable]
    public partial class EnumElement : IPure
    {
        public string Name { get; }
        public ImmutableArray<EnumElementField> Fields { get; }

        public void EnsurePure()
        {
            Misc.EnsurePure(Fields);
        }
    }

    [AutoConstructor, ImplementIEquatable]
    public partial class EnumDecl : Decl
    {   
        public string Name { get; }
        public ImmutableArray<string> TypeParams { get; }
        public ImmutableArray<EnumElement> Elems { get; }

        public override void EnsurePure()
        {
            Misc.EnsurePure(TypeParams);
            Misc.EnsurePure(Elems);
        }
    }    
}
