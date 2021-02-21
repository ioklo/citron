using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;

namespace Gum.IR0
{
    public class EnumElement
    {
        public string Name { get; }
        public ImmutableArray<TypeAndName> Params { get; }

        public EnumElement(string name, ImmutableArray<TypeAndName> parameters)
        {
            Name = name;
            Params = parameters;
        }        
    }

    public class EnumDecl : TypeDecl
    {
        public string Name { get; }
        public ImmutableArray<string> TypeParams { get; }
        public ImmutableArray<EnumElement> Elems { get; }

        public EnumDecl(TypeDeclId id, string name, ImmutableArray<string> typeParams, ImmutableArray<EnumElement> elems)
            : base(id)
        {
            Name = name;
            TypeParams = typeParams;
            Elems = elems;
        }
    }    
}
