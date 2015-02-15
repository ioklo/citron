using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParserGenerator.AST
{
    class OptionalNode : ExpNode
    {
        public ExpNode E { get; set; }

        public OptionalNode(ExpNode e)
        {            
            E = e;
        }
    }
}
