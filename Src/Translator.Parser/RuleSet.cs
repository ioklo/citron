using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gum.Translator.Parser
{
    // 여러 룰들을 가지고 있는다
    public class RuleSet
    {
        List<Rule> rules = new List<Rule>();
        Dictionary<Rule, int> ruleOrders = new Dictionary<Rule,int>();

        public RuleSet(params Rule[] rules)
        {
            this.rules.AddRange(rules);
        }

        public IEnumerable<Rule> GetAvailableRules(Symbol symbol)
        {
            foreach (var rule in rules)
            {               
                
                foreach (var pos in rule.StartPositions)
                {
                    if (pos.IsAvailable(symbol))
                    {
                        yield return rule;
                        break;
                    }
                }
            }
        }


        
        public IEnumerable<Trace> GetAvailableTraces(Symbol symbol)
        {   
            foreach(var rule in rules)
            {
                foreach (var pos in rule.StartPositions)
                {
                    if (pos.IsAvailable(symbol))
                        yield return new Trace(null, pos);
                }                
            }
        }

        int curOrder = 0;
        public void AddOrder(params Rule[] rules)
        {
            foreach (var rule in rules)
                ruleOrders.Add(rule, curOrder);
            curOrder++;
        }

        public int? Compare(Rule rule1, Rule rule2)
        {
            int order1, order2;
            if (!ruleOrders.TryGetValue(rule1, out order1))
                return null;

            if (!ruleOrders.TryGetValue(rule2, out order2))
                return null;

            return order1 - order2;
        }
    }
}
