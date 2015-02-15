using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParserGenerator.AST
{
    class VariableNode : LookaheadExpNode
    {
        public string ID { get; private set; }

        public VariableNode(string id)
        {
            ID = id;
        }
    }
}
