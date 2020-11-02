using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;

namespace Gum.IR0
{
    public abstract partial class TypeDecl
    {
        public class EnumElement
        {
            public string Name { get; }
            public ImmutableArray<TypeAndName> Params { get; }

            public EnumElement(string name, IEnumerable<TypeAndName> parameters)
            {
                Name = name;
                Params = parameters.ToImmutableArray();
            }

            public EnumElement(string name, params TypeAndName[] parameters)
            {
                Name = name;
                Params = ImmutableArray.Create(parameters);
            }
        }

        public class Enum : TypeDecl
        {
            public string Name { get; }
            public ImmutableArray<string> TypeParams { get; }
            public ImmutableArray<EnumElement> Elems { get; }

            public Enum(TypeDeclId id, string name, IEnumerable<string> typeParams, IEnumerable<EnumElement> elems)
                : base(id)
            {
                Name = name;
                TypeParams = typeParams.ToImmutableArray();
                Elems = elems.ToImmutableArray();
            }

            public Enum(TypeDeclId id, string name, IEnumerable<string> typeParams, params EnumElement[] elems)
                : base(id)
            {
                Name = name;
                TypeParams = typeParams.ToImmutableArray();
                Elems = ImmutableArray.Create(elems);
            }
        }
    }
}
