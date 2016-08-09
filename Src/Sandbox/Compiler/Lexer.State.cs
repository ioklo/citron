using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gum.Compiler
{
    partial class Lexer
    {
        public struct State
        {
            public int StartIndex { get; private set; }
            public int TokenLength { get; private set; }
            public TokenType TokenType { get; private set; }

            public State(int s, int t, TokenType k)
            {
                StartIndex = s;
                TokenLength = t;
                TokenType = k;
            }
        }
    }
}
