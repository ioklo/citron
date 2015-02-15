using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gum.Translator.Parser
{
    // 토큰 단위의 룰, 
    public abstract class Terminal : Symbol
    {
        public Terminal(string id) : base(id)
        {
        }

        public abstract bool Accept(string input, int cur, out int next);
    }
}
