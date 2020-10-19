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

    public class Enum
    {
        public string Name { get; }
        public ImmutableArray<string> TypeParams { get; }
        public ImmutableArray<EnumElement> Elems { get; }
        public Enum(string name, IEnumerable<string> typeParams, IEnumerable<EnumElement> elems)
        {
            Name = name;
            TypeParams = typeParams.ToImmutableArray();
            Elems = elems.ToImmutableArray();
        }

        public Enum(string name, IEnumerable<string> typeParams, params EnumElement[] elems)
        {
            Name = name;
            TypeParams = typeParams.ToImmutableArray();
            Elems = ImmutableArray.Create(elems);
        }
    }
}
