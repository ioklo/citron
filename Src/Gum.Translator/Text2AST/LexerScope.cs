using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gum.Translator.Text2AST
{
    public class LexerScope : IDisposable
    {
        Lexer lexer;
        LexerState state;
        bool bAccept = false;

        public LexerScope(Lexer lexer)
        {
            this.lexer = lexer;
            state = lexer.GetState();
        }

        public void Accept()
        {
            bAccept = true;
        }

        public void Dispose()
        {
            if (!bAccept)
                lexer.SetState(state);
        }
    }
}
