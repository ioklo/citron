using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gum.Translator.Parser
{
    public abstract class ASTNode
    {
        public Symbol Symbol { get; private set; }
        public ASTNode(Symbol symbol)
        {
            Symbol = symbol;
        }

        public override string ToString()
        {
            return String.Format("Node[{0}]", Symbol);
        }

        public abstract String ToString(string indent);
    }
}
