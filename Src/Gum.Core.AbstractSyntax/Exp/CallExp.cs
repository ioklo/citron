using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gum.Core.AbstractSyntax
{
    public class CallExp : IExp
    {
        public IExp FuncExp { get; private set; }
        public List<IExp> Args { get; private set; }

        public CallExp(IExp funcExp)
        {
            FuncExp = funcExp;
            Args = new List<IExp>();
        }

        public void Visit(IExpVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
}
