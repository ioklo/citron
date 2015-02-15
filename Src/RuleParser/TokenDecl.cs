using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace RuleParser
{
    // 토큰에 대한 명세
    public class TokenDecl
    {
        public string Name { get; private set; } 
        
        // regex인지, string인지
        public string Exp { get; private set; }
        public bool IsRegex { get { return regex != null; } }

        private Regex regex;

        public TokenDecl(string name, string exp, bool bRegex)
        {
            Name = name;
            Exp = exp;

            // 부분문자열을 위해 "\G" 추가.
            regex = bRegex ? new Regex(@"\G" + exp, RegexOptions.Multiline) : null;
        }

        public int Accept(string input, int startIdx)
        {
            if (regex != null)
            {
                var match = regex.Match(input, startIdx);

                if (match.Success)
                    return match.Index + match.Length;
                else
                    return -1;
            }
            else
            {
                // string 비교
                if (input.Length < startIdx + Exp.Length)
                    return -1;

                if (input.Substring(startIdx, Exp.Length) == Exp)
                    return startIdx + Exp.Length;
            }            

            return -1;
        }
    }
}
