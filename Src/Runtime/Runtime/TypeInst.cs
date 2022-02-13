using Citron.CompileTime;
using System;
using System.Collections.Generic;
using System.Text;

namespace Citron.Runtime
{
    public abstract class TypeInst
    {
        public abstract ITypeSymbol GetTypeValue();
        public abstract Value MakeDefaultValue();
    }
}
