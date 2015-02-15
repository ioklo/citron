using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gum.Translator.Parser
{
    public class RuleWithPriority
    {
        public Rule Rule { get; private set; }
        public int Priority { get; private set; }

        public RuleWithPriority(Rule rule, int priority)
        {
            Rule = rule;
            Priority = priority;
        }
    }
}
