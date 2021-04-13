using System;
using System.Collections.Generic;
using System.Text;

namespace Gum
{
    public struct LexerContext
    {
        public static LexerContext Make(BufferPosition pos)
        {
            return new LexerContext(pos);
        }
        
        public BufferPosition Pos { get; }

        private LexerContext(BufferPosition pos) 
        {   
            Pos = pos; 
        }
        
        public LexerContext UpdatePos(BufferPosition pos)
        {
            return new LexerContext(pos);
        }
    }
}
