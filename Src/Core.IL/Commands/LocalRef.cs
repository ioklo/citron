using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gum.Core.IL.Commands
{
    // 현재 함수의 로컬 변수의 위치를 로컬 변수에 담아줍니다
    public class LocalRef : ICommand
    {
        public int DestReg { get; private set; }
        public int Index { get; private set; }

        public LocalRef(int destReg, int index)
        {
            DestReg = destReg;
            Index = index;
        }

        public void Visit(ICommandVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
}
