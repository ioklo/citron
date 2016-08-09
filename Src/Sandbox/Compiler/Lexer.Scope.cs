using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gum.Compiler
{
    partial class Lexer
    {
        public class Scope : IDisposable
        {
            Lexer lexer;
            State state;
            bool bAccept = false;

            public Scope(Lexer lexer)
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
}
