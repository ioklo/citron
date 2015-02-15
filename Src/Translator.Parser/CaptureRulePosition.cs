using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gum.Translator.Parser
{
    public class CaptureRulePosition : IRulePosition
    {
        private Rule captureRule;
        public ExpPosition Position { get; private set; }

        public CaptureRulePosition(Rule captureRule, ExpPosition pos)
        {
            this.captureRule = captureRule;
            this.Position = pos;
        }

        public bool IsAvailable(Symbol symbol)
        {
            return Position.IsAvailable(symbol);            
        }

        public IEnumerable<IRulePosition> Consume(ASTNode node)
        {
            if (!Position.IsAvailable(node.Symbol)) yield break;
            foreach(var pos in Advance(Position.StepIns()))
            {
                yield return new CaptureRulePosition(captureRule, pos);
            }
        }

        // Advance
        public IEnumerable<ExpPosition> Advance(IEnumerable<ExpPosition> startPos)
        {
            var added = new HashSet<ExpPosition>(startPos);
            var curPosSet = new HashSet<ExpPosition>(startPos);
            
            while(curPosSet.Count != 0)
            {
                var nextPosSet = new HashSet<ExpPosition>();

                foreach(var pos in curPosSet)
                {
                    if (pos == ExpPosition.Empty)
                    {
                        yield return pos;
                        continue;
                    }

                    if (pos.Exp is SymbolExp)
                    {
                        yield return pos;
                        continue;
                    }

                    foreach( var next in pos.StepIns())
                    {
                        if (added.Contains(next)) continue;

                        added.Add(next);
                        nextPosSet.Add(next);
                    }
                }

                curPosSet = nextPosSet;
            }
        }

        public static IEnumerable<IRulePosition> CreatePositions(Rule rule, Exp exp)
        {
            var added = new HashSet<ExpPosition>();
            var curPosSet = new HashSet<ExpPosition>();

            var startPos = new ExpPosition(exp);            
            added.Add(startPos);
            curPosSet.Add(startPos);
            
            while(curPosSet.Count != 0)
            {
                var nextPosSet = new HashSet<ExpPosition>();

                foreach(var pos in curPosSet)
                {
                    if (pos == ExpPosition.Empty) continue;
                    if (pos.Exp is SymbolExp)
                    {
                        yield return new CaptureRulePosition(rule, pos);
                        continue;
                    }

                    foreach( var next in pos.StepIns())
                    {
                        if (added.Contains(next)) continue;

                        added.Add(next);
                        nextPosSet.Add(next);
                    }
                }

                curPosSet = nextPosSet;
            }
        }


        public bool IsReducible
        {
            get { return Position == ExpPosition.Empty; }
        }

        public bool IsShiftable
        {
            get { return Position != ExpPosition.Empty; }
        }

        public Rule Rule
        {
            get { return captureRule; }
        }

        public override string ToString()
        {
            return Position.ToString();
        }
    }
}
