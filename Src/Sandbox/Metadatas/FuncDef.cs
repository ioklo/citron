using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gum.Metadatas
{
    class FuncDef : MetadataComponent
    {
        public int TypeVarCount { get; private set; } // Variadic 이 가능해야 한다.

        public IType ReturnType { get; private set; }
        public IReadOnlyList<IType> ArgTypes { get; private set; }

        public FuncDef(Namespace ns, string name, int typeVarCount, IType returnType, IEnumerable<IType> argTypes )
            : base(ns, name)
        {
            TypeVarCount = typeVarCount;
            ReturnType = returnType;
            ArgTypes = argTypes.ToList();
        }
    }
}
