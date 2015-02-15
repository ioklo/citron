using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParserGenerator.AST
{
    class ZeroOrMoreNode : ExpNode
    {
        public ExpNode E { get; private set; }

        public ZeroOrMoreNode(ExpNode e)
        {
            
            E = e;
        }
    }
}
