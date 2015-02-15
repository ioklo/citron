using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RuleParser
{
    class Program
    {
        // 앞에서 부터 순서대로 decls
        static IEnumerable<Token> GetTokens(IList<TokenDecl> skipDecls, IList<TokenDecl> tokenDecls, string input)
        {
            int cur = 0;

            while(cur < input.Length)
            {
                bool bFound = false;

                foreach (var skipDecl in skipDecls)
                {
                    int end = skipDecl.Accept(input, cur);
                    if (end != -1)
                    {
                        cur = end;
                        bFound = true;
                        break;
                    }
                }

                if (bFound) continue;

                foreach (var tokenDecl in tokenDecls)
                {
                    int end = tokenDecl.Accept(input, cur);
                    if (end != -1)
                    {
                        yield return new Token(tokenDecl.Name, cur, end - cur);
                        cur = end;
                        bFound = true;
                        break;
                    }
                }

                if (!bFound) break;
            }
        }

        static void Main(string[] args)
        { 
            var rules = Rules.CreatePseudoRule();
            // var ruleDeclsRule = rules.GetRule("ruleDecls");

            string input; //= @"    int _thisIsIdentifier9 = 83; // it is comment
//int a = 7; ";
            using (var stream = new StreamReader(@"..\..\spec.txt"))
            {
                input = stream.ReadToEnd();
                // rules.Parse("ruleDecls", str);
            }

            // skip 토큰도 있어야 합니다.
            IList<TokenDecl> skipDecls = new List<TokenDecl>()
            {
                new TokenDecl("COMMENT", @"//.*$", true),
                new TokenDecl("WHITESPACE", @"\s+", true),
            };

            IList<TokenDecl> tokenDecls = new List<TokenDecl>()
            {
                new TokenDecl("BAR", @"|", false),
                new TokenDecl("RID", @"#[_a-zA-Z][_A-Za-z0-9]*", true),
                new TokenDecl("ARROW", @"=>", false),
                new TokenDecl("STRING", @"\""(?:[^\""\\]|\\.)*\""", true),
                new TokenDecl("REGEX", @"r\""(?:[^\""\\]|\\.)*\""", true),

                new TokenDecl("ID", @"[_a-zA-Z][_A-Za-z0-9]*", true),

                new TokenDecl("TOKENID", @"<[_a-zA-Z][_A-Za-z0-9]*>", true),
                new TokenDecl("EQUAL", @"=", false),
                new TokenDecl("LT", @"<", false),
                new TokenDecl("RT", @">", false),
            };
            
            foreach (Token token in GetTokens(skipDecls, tokenDecls, input))
            {
                Console.WriteLine("{0}: {1}", token.Name, input.Substring(token.StartIdx, token.Length));
            }

            // var ast = mainRuleExp.Parse("spec.txt");
        }
    }
}
