using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParserGenerator.AST
{
    class OneOrMoreNode : ExpNode
    {
        public ExpNode E { get; private set; }

        public OneOrMoreNode(ExpNode e)
        {
            E = e;
        }
    }
}
