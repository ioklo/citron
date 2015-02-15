using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gum.Translator.Parser
{
    public class TokenNode : ASTNode
    {
        public string Text { get; private set; }

        public TokenNode(Symbol symbol, string text) : base(symbol)
        {
            Text = text;
        }

        public override string ToString(string indent)
        {
            return indent + Text;
        }
    }
}
