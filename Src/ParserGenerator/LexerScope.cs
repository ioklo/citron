using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ParserGenerator
{
    class LexerScope : IDisposable
    {
        private Lexer lexer;
        public int Pos { get; private set; }
        public int Len { get; private set; }
        public Token Kind { get; private set; }
        public string Tag { get; private set; }

        bool bAccept = false;

        public LexerScope(Lexer lexer, int pos, int len, Token kind, string tag)
        {
            // TODO: Complete member initialization
            this.lexer = lexer;
            this.Pos = pos;
            this.Len = len;
            this.Kind = kind;
            this.Tag = tag;
        }

        public void Accept()
        {
            lexer.AcceptScope(this);
            Console.WriteLine("Accept {0}", Tag);
            bAccept = true;
        }

        public void Dispose()
        {
            if (!bAccept)
            {
                Console.WriteLine("Cancel {0}", Tag);
                lexer.CancelScope(this);
            }
        }
    }
}
