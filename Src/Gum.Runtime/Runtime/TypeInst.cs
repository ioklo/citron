using Gum.CompileTime;
using System;
using System.Collections.Generic;
using System.Text;

namespace Gum.Runtime
{
    public abstract class TypeInst
    {
        public abstract TypeValue GetTypeValue();
        public abstract Value MakeDefaultValue();
    }
}
