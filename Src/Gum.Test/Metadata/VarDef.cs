using Gum.Test.TypeInst;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gum.Test.Metadata
{
    class VarDef : MetadataComponent
    {
        public ITypeInst TypeInst {get; private set;}

        public VarDef(Namespace ns, ITypeInst typeInst, string name)
            : base(ns, name)
        {
            TypeInst = typeInst;
        }
    }
}
