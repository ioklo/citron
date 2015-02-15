using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RuleParser
{
    class Token
    {
        public string Name { get; private set; }
        public int StartIdx { get; private set; }
        public int Length { get; private set; }

        public Token(string declName, int startIdx, int length)
        {
            this.Name = declName;
            this.StartIdx = startIdx;
            this.Length = length;
        }
    }
}
