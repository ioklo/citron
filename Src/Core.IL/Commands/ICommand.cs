using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gum.Core.IL.Commands
{
    public interface ICommand
    {
        void Visit(ICommandVisitor visitor);
    }
}
