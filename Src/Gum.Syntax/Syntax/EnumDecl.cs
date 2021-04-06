using Gum.Syntax;
using System;
using System.Collections.Generic;
using Gum.Collections;
using System.Linq;
using System.Text;

namespace Gum.Syntax
{
    public class EnumDeclElement : ISyntaxNode
    {
        public string Name { get; }
        public ImmutableArray<TypeAndName> Params { get; }        

        public EnumDeclElement(string name, ImmutableArray<TypeAndName> parameters)
        {
            Name = name;
            Params = parameters;
        }
    }

    public class EnumDecl : TypeDecl
    {
        public ImmutableArray<string> TypeParams { get; }
        public ImmutableArray<EnumDeclElement> Elems { get; }

        public override int TypeParamCount { get => TypeParams.Length; }

        public EnumDecl(string name, ImmutableArray<string> typeParams, ImmutableArray<EnumDeclElement> elems)
            : base(name)
        {
            TypeParams = typeParams;
            Elems = elems;
        }
    }
}
