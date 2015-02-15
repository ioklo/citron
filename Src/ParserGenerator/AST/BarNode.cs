using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParserGenerator.AST
{
    class BarNode : LookaheadExpNode
    {
        public List<ExpNode> Exps { get; private set; }

        public BarNode(List<ExpNode> exps)
        {
            Exps = exps;
        }
    }
}
