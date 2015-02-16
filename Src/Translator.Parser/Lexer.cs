using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gum.Translator.Parser
{
    public class Lexer
    {
        List<Terminal> skipTokens = new List<Terminal>();
        List<Terminal> tokens = new List<Terminal>();

        public void AddSkipToken(params Terminal[] rules)
        {
            skipTokens.AddRange(rules);
        }

        public void AddToken(params Terminal[] rules)
        {
            tokens.AddRange(rules);
        }

        public IEnumerable<TokenNode> Lex(string input)
        {
            int cur = 0;
            while (cur < input.Length)
            {
                // rule 중에서 만족하는 input을 찾는다
                bool bFound = false;
                foreach (var skipToken in skipTokens)
                {
                    int next;
                    if (skipToken.Accept(input, cur, out next))
                    {
                        cur = next;
                        bFound = true;
                        break;
                    }
                }
                if (bFound) continue;

                foreach (var token in tokens)
                {
                    int next;
                    if (token.Accept(input, cur, out next))
                    {
                        yield return new TokenNode(token, input.Substring(cur, next - cur));
                        cur = next;
                        bFound = true;
                        break;
                    }
                }

                if (!bFound) throw new Exception();
            }
        }
    }
}
