using Gum.CompileTime;
using Gum.Runtime;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Gum.Runtime
{
    public class RuntimeModuleMisc
    {
        public delegate ValueTask Invoker(DomainService domainService, TypeArgumentList typeArgList, Value? thisValue, IReadOnlyList<Value> args, Value result);
    }
}
