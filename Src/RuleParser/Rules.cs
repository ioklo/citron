using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RuleParser
{
    class Rules
    {
        public static Rules CreatePseudoRule()
        {
            Rule declList = new Rule();
            declList.AddEntry(new TokenExp("#EOF"));
            declList.AddEntry(new IDExp("decl"), new IDExp("declList"));

            Rule decl = new Rule();
            decl.AddEntry(new IDExp("ruleDecl"));
            decl.AddEntry(new IDExp("tokenDecl"));

            Rule tokenDecl = new Rule();
            tokenDecl.AddEntry(new TokenExp("TOKENID"), new TokenExp("EQUAL"), new IDExp("tokenExp"));

            Rule tokenExp = new Rule();
            tokenExp.AddEntry(new TokenExp("STRING"));
            tokenExp.AddEntry(new TokenExp("REGEX"));

            Rule ruleDecl = new Rule();
            ruleDecl.AddEntry(new TokenExp("RID"), new TokenExp("EQUAL"), new IDExp("ruleEntries"));
            ruleDecl.AddEntry(new TokenExp("RID"), new TokenExp("EQUAL"), new TokenExp("BAR"), new IDExp("ruleEntries"));

            Rule ruleEntries = new Rule();
            ruleEntries.AddEntry(new IDExp("ruleEntry"));
            ruleEntries.AddEntry(new IDExp("ruleEntry"), new TokenExp("BAR"), new IDExp("ruleEntries"));

            Rule ruleEntry = new Rule();
            ruleEntry.AddEntry("Tag", new IDExp("id"), new TokenExp("ARROW"), new IDExp("IDExps"));
            ruleEntry.AddEntry(new IDExp("IDExps"));

            Rule ruleExps = new Rule();
            ruleExps.AddEntry(new IDExp("ruleExp"));
            ruleExps.AddEntry(new IDExp("ruleExp"), new IDExp("ruleExps"));

            // rule은 이름, RuleEntry list
            Rule ruleExp = new Rule();
            ruleExp.AddEntry("ID", new TokenExp("ID"));
            ruleExp.AddEntry("Token", new TokenExp("TOKENID"));

            Rules rules = new Rules();
            rules.Add("declList", declList);
            rules.Add("decl", decl);
            rules.Add("tokenDecl", tokenDecl);
            rules.Add("tokenExp", tokenExp);
            rules.Add("ruleDecl", ruleDecl);
            rules.Add("ruleEntries", ruleEntries);
            rules.Add("ruleEntry", ruleEntry);
            rules.Add("ruleExps", ruleExps);
            rules.Add("ruleExp", ruleExp);

            return rules;
        }

        private void Add(string tag, Rule rule)
        {
            rules.Add(tag, rule);
        }

        Dictionary<string, Rule> rules = new Dictionary<string, Rule>();

        public Rule GetRule(string name)
        {
            Rule rule;
            if (!rules.TryGetValue(name, out rule))
                return null;

            return rule;
        }

        public bool Parse(string ruleName, string str, out AST ast)
        {
            Rule rule = GetRule(ruleName);
            if (rule == null)
            {
                ast = null;
                return false;
            }

            ast = null;

            foreach (RuleEntry entry in rule.Entries)
            {
                AST result;
                if (entry.Parse(str, out result))
                {
                    if (ast != null)
                        throw new Exception("it can be parsed by multiple ways");

                    ast = result;
                }
            }

            return ast != null;
        }
    }
}
