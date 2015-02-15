using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParserGenerator.AST
{
    class ModuleNode
    {
        public List<TokenDeclNode> TokenDeclNodes { get; private set; }
        public List<GroupDeclNode> GroupDeclNodes { get; private set; }

        public ModuleNode(List<TokenDeclNode> tokenDeclNodes, List<GroupDeclNode> groupDeclNodes)
        {
            TokenDeclNodes = tokenDeclNodes;
            GroupDeclNodes = groupDeclNodes;
        }
    }
}
