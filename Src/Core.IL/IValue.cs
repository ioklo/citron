using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gum.Core.IL
{
    // Stack에 들어갈 수 있는 값
    public interface IValue
    {
        void CopyFrom(IValue value);
    }
}
