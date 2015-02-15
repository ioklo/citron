using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParserGenerator.AST
{
    class GroupDeclNode
    {
        public string Name { get; private set; }
        public List<GroupDeclNode> ChildGroupDecls { get; private set; }
        public List<NodeDeclNode> ChildNodeDecls { get; private set; }
        public LookaheadExpNode LookaheadExpNode { get; private set; }

        public GroupDeclNode(string name, List<GroupDeclNode> childGroupDecls, List<NodeDeclNode> childNodeDecls, LookaheadExpNode lookaheadExpNode)
        {
            Name = name;
            ChildGroupDecls = childGroupDecls;
            ChildNodeDecls = childNodeDecls;
            LookaheadExpNode = lookaheadExpNode;
        }
    }
}
