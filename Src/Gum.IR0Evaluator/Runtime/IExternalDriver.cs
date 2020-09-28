using Gum.CompileTime;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Gum.Runtime
{
    public delegate ValueTask ExternalFuncDelegate(Value? retValue, params Value[] values);

    public interface IExternalDriver
    {
        ExternalFuncDelegate GetDelegate(ExternalDriverFuncId id);
    }
}
