using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gum.Translator.Text2AST
{
    public struct LexerState
    {
        public readonly int startIdx;
        public readonly int tokenLen;
        public readonly TokenType kind;

        public LexerState(int s, int t, TokenType k)
        {
            startIdx = s;
            tokenLen = t;
            kind = k;
        }
    }
}
