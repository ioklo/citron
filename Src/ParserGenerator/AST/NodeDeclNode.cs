using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParserGenerator.AST
{
    class NodeDeclNode
    {
        public string ID { get; private set; }
        public List<ArgNode> ArgNodes { get; private set; }
        public ExpNode ExpNode { get; private set; }
        public LookaheadExpNode LookaheadExpNode { get; private set; }

        public NodeDeclNode(string id, List<ArgNode> argNodes, ExpNode expNode, LookaheadExpNode lookaheadExpNode)
        {
            // TODO: Complete member initialization
            ID = id;
            ArgNodes = argNodes;
            ExpNode = expNode;
            LookaheadExpNode = lookaheadExpNode;
        }
    }
}
