using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RuleParser
{
    class RuleEntry
    {
        public string TagName { get; private set; }
        public Exp[] RuleExps { get; private set; }

        public RuleEntry(string tagName, params Exp[] ruleExps)
        {
            TagName = tagName;
            RuleExps = ruleExps;
        }

        private bool InnerParse(int expIdx, string str)
        {
            if (RuleExps.Length == expIdx)
                return true;

            // 
            for (int t = str.Length; t > 0; t++)
            {
                /*if (RuleExps[expIdx].Parse(str.Substring(0, t), ast) &&
                    InnerParse(rule, ruleIdx + 1, str.Substring(t)))
                {
                    return true;
                }*/
            }

            return false;
        }

        public bool Parse(string str, out AST result)
        {
            result = null;
            for (int t = str.Length; t > 0; t++)
            {
                string substr = str.Substring(0, t);

                foreach (Exp exp in RuleExps)
                {
                    // AST ast;
                    //if (!exp.Parse(substr, out ast)) break;

                    substr = str.Substring(t);
                }

            }

            return false;

        }
    }
}
