using Gum.Syntax;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;

namespace Gum.Syntax
{
    public class EnumDeclElement : ISyntaxNode
    {
        public string Name { get; }
        public ImmutableArray<TypeAndName> Params { get; }        

        public EnumDeclElement(string name, IEnumerable<TypeAndName> parameters)
        {
            Name = name;
            Params = parameters.ToImmutableArray();
        }

        public EnumDeclElement(string name, params TypeAndName[] parameters)
        {
            Name = name;
            Params = ImmutableArray.Create(parameters);
        }
    }

    public class EnumDecl : ISyntaxNode
    {
        public string Name { get; }
        public ImmutableArray<string> TypeParams { get; }
        public ImmutableArray<EnumDeclElement> Elems { get; }

        public EnumDecl(string name, IEnumerable<string> typeParams, IEnumerable<EnumDeclElement> elems)
        {
            Name = name;
            TypeParams = typeParams.ToImmutableArray();
            Elems = elems.ToImmutableArray();
        }

        public EnumDecl(string name, IEnumerable<string> typeParams, params EnumDeclElement[] elems)
        {
            Name = name;
            TypeParams = typeParams.ToImmutableArray();
            Elems = ImmutableArray.Create(elems);
        }        
    }
}
