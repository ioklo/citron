using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gum.Core.AbstractSyntax
{
    // a . b
    public class FieldExp : IExp
    {
        public IExp Exp { get; private set; }
        public string ID { get; private set; }

        public FieldExp(IExp exp, string id)
        {
            Exp = exp;
            ID = id;
        }

        public void Visit(IExpVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
}
