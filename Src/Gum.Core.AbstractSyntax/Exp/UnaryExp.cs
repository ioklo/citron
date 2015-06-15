using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gum.Core.AbstractSyntax
{
    public class UnaryExp : IExp
    {
        public UnaryExpKind Operation { get; set; }
        public IExp Operand { get; set; }

        public void Visit(IExpVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
}
