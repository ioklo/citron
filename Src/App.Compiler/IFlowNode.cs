using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gum.App.Compiler
{
    public interface IFlowNode
    {
        void SetNext(IFlowNode nextNode);
    }
}
