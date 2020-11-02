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
            public Type Type { get; }
            public IR0.Exp? InitExp { get; }

            public Element(string name, Type type, IR0.Exp? initExp)
            {
                Name = name;
                Type = type;
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