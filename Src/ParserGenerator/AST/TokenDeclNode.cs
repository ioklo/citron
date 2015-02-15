using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParserGenerator.AST
{
    class TokenDeclNode
    {
        public string Name { get; private set; }
        public string Str { get; private set; }
        public string Regex { get; private set; }

        public TokenDeclNode(string name, string str, string regex)
        {
            Name = name;
            Str = str;
            Regex = regex;
        }
    }
}
