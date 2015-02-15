using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ParserGenerator.AST
{
    class ExpSequenceNode : ExpNode
    {
        public List<ExpNode> Exps { get; private set; }

        public ExpSequenceNode(List<ExpNode> exps)
        {
            Exps = exps;
        }
    }
}
