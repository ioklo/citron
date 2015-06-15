using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gum.Core.IL
{
    // type information
    public interface IType
    {
        IReadOnlyList<IType> Fields { get; }
    }
}
