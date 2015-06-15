using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gum.Core.AbstractSyntax
{
    public class AssignExp : IExp
    {
        public IExp Left { get; set; }
        public IExp Exp { get; set; }

        public void Visit(IExpVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
}
