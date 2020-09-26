using Gum.CompileTime;
using System;
using System.Collections.Generic;
using System.Text;

namespace Gum.Runtime
{
    public delegate void ExternalFuncDelegate(Value? retValue, params Value[] values);

    public interface IExternalDriver
    {
        ExternalFuncDelegate GetDelegate(ExternalDriverFuncId id);
    }
}
