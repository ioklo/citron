using Gum.Test.TypeInst;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gum.Test.Metadata
{
    class FuncDef : MetadataComponent
    {
        public int TypeVarCount { get; private set; } // Variadic 이 가능해야 한다.

        public ITypeInst ReturnType { get; private set; }
        public IReadOnlyList<ITypeInst> ArgTypes { get; private set; }

        public FuncDef(Namespace ns, string name, int typeVarCount, ITypeInst returnType, IEnumerable<ITypeInst> argTypes )
            : base(ns, name)
        {
            TypeVarCount = typeVarCount;
            ReturnType = returnType;
            ArgTypes = argTypes.ToList();
        }
    }
}
