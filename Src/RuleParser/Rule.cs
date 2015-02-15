using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RuleParser
{
    class Rule
    {
        public List<TokenDecl> TokenDecls { get; private set; }
        public List<RuleEntry> Entries { get; private set; }

        public Rule()
        {
            Entries = new List<RuleEntry>();
            TokenDecls = new List<TokenDecl>();
        }

        public void AddEntry(params Exp[] exps)
        {
            Entries.Add(new RuleEntry(null, exps));
        }

        public void AddEntry(string tagName, params Exp[] exps)
        {
            Entries.Add(new RuleEntry(tagName, exps));
        }
    }
}
