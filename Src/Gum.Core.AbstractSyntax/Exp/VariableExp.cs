using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gum.Core.AbstractSyntax
{
    public class VariableExp : IExp
    {
        public string Name { get; private set; }

        public VariableExp(string name)
        {
            Name = name;
        }

        public void Visit(IExpVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
}
