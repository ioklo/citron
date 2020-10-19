using Gum.CompileTime;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Gum.IR0
{
    public class LocalVarDecl
    {
        public struct Element
        {
            public string Name { get; }
            public TypeId TypeId { get; }
            public IR0.Exp? InitExp { get; }

            public Element(string name, TypeId typeId, IR0.Exp? initExp)
            {
                Name = name;
                TypeId = typeId;
                InitExp = initExp;
            }
        }

        public ImmutableArray<Element> Elems { get; }

        public LocalVarDecl(IEnumerable<Element> elems)
        {
            Elems = elems.ToImmutableArray();
        }
    }    
}