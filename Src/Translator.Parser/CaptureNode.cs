using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gum.Translator.Parser
{
    class CaptureNode : ASTNode
    {
        public Dictionary<string, List<ASTNode>> Children { get; private set; }
        public Rule Rule { get; private set; }

        public CaptureNode(Rule rule) : base(rule.ReduceSymbol)
        {
            Children = new Dictionary<string, List<ASTNode>>();
            Rule = rule;
        }

        public void Add(string variable, ASTNode node)
        {
            List<ASTNode> nodes;
            if(!Children.TryGetValue(variable, out nodes))
            {
                nodes = new List<ASTNode>();
                Children.Add(variable, nodes);
            }

            nodes.Add(node);
        }
        
        public override string ToString(string indent)
        {
            var sb = new StringBuilder();

            sb.AppendLine(indent + "{");            
            sb.AppendFormat(indent + "    rule:\"{0}\",", Rule.ID);
            sb.AppendLine();

            foreach(var kv in Children)
            {
                sb.AppendFormat(indent + "    {0}: [", kv.Key);
                sb.AppendLine();
                foreach(var node in kv.Value)
                {
                    sb.AppendFormat("{0},", (node.ToString(indent + "        ")));
                    sb.AppendLine();
                }

                sb.AppendLine(indent + "    ],");
            }
            sb.Append(indent + "}");

            return sb.ToString();
        }
    }
}
