using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gum.Translator.Parser
{
    public class StringTokenType : Terminal
    {
        string token;

        public StringTokenType(string id, string token) : base(id)
        {
            this.token = token;
        }

        public override bool Accept(string input, int cur, out int next)
        {
            if (input.Substring(cur, token.Length) == token)
            {
                next = cur + token.Length;
                return true;
            }

            next = cur;
            return false;
        }
    }
}
