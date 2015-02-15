using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Gum.Translator.Parser
{
    public class RegexTokenType : Terminal
    {
        Regex regex;

        public RegexTokenType(string id, string pattern) : base(id)
        {
            regex = new Regex(@"\G" + pattern);
        }

        public override bool Accept(string input, int cur, out int next)
        {
            var match = regex.Match(input, cur);
            if (!match.Success)
            {
                next = cur;
                return false;
            }

            next = cur + match.Length;
            return true;
        }
    }
}
