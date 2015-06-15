using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gum.Core.IL
{
    public interface IFunction
    {
        string Name { get; }
        IType RetType { get; }
        IReadOnlyList<IType> ArgTypes { get; }
    }
}
