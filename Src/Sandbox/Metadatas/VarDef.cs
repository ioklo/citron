using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gum.Metadatas
{
    class VarDef : MetadataComponent
    {
        public IType Type { get; private set; }

        public VarDef(Namespace ns, IType type, string name)
            : base(ns, name)
        {
            Type = type;
        }
    }
}
