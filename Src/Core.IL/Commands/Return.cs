using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gum.Core.IL.Commands
{
    // 스택 맨 위에 있는 값을 돌려준다.
    // Value
    public class Return : ICommand
    {
        public int Value { get; private set; }

        public Return(int value)
        {
            Value = value;
        }

        public void Visit(ICommandVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
}
