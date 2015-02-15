using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gum.Translator.Parser
{
    public class Rule
    {
        public string ID { get; private set; }
        public Associativity Assoc { get; private set; }

        public NonTerminal ReduceSymbol { get; private set; }
        public Exp Exp { get; private set; }

        public Rule(string id, NonTerminal reduceSymbol, Exp exp, Associativity assoc = Associativity.None)
        {
            ID = id;
            ReduceSymbol = reduceSymbol;
            Exp = exp;
            Assoc = assoc;
        }        

        class GetFirstSymbolsAndEmpty : IExpVisitor<IEnumerable<Symbol>, Object>
        {
            public IEnumerable<Symbol> Visit(StarExp exp, object a)
            {
                foreach (var symbol in exp.E.Visit(this, null))
                    yield return symbol;
            }

            public IEnumerable<Symbol> Visit(PlusExp exp, object a)
            {
                foreach (var symbol in exp.E.Visit(this, null))
                    yield return symbol;
            }

            public IEnumerable<Symbol> Visit(OptionalExp exp, object a)
            {
                foreach (var symbol in exp.E.Visit(this, null))
                    yield return symbol;
            }

            public IEnumerable<Symbol> Visit(SequenceExp exp, object a)
            {
                throw new NotImplementedException();
            }

            public IEnumerable<Symbol> Visit(SymbolExp exp, object a)
            {
                yield return exp.Symbol;
            }

            public static IEnumerable<Symbol> Apply(Exp e)
            {
                var visitor = new GetFirstSymbolsAndEmpty();
                return e.Visit(visitor, null);
            }
        }

        public IEnumerable<Symbol> FirstSymbol
        {
            get
            {
                return GetFirstSymbolsAndEmpty.Apply(Exp);
            }
        }

        public IEnumerable<IRulePosition> StartPositions
        {
            get
            {
                return CaptureRulePosition.CreatePositions(this, Exp);
            }
        }

        public bool Reduce(ShiftStack shiftStack, Trace trace, out ASTNode node)
        {
            node = null;

            var captureRuleNode = new CaptureNode(this);

            trace = trace.PrevTrace;
            while (trace != null)
            {
                var item = shiftStack.Pop();

                var captureRulePos = trace.Position as CaptureRulePosition;
                if (captureRulePos == null) return false;

                var symbolExp = captureRulePos.Position.Exp as SymbolExp;
                if (symbolExp == null) return false;

                if (symbolExp.CaptureVar != null)
                {
                    List<ASTNode> nodes;
                    if (!captureRuleNode.Children.TryGetValue(symbolExp.CaptureVar, out nodes))
                    {
                        nodes = new List<ASTNode>();
                        captureRuleNode.Children.Add(symbolExp.CaptureVar, nodes);
                    }

                    if (symbolExp.Symbol != item.Node.Symbol) return false;

                    nodes.Add(item.Node);
                }

                trace = trace.PrevTrace;
            }

            node = captureRuleNode;
            return true;
        }

        public override string ToString()
        {
            return ID;
        }

        // type exp = Exp
        //          | OneOrMore
        //          | ZeroOrMore
        //          | 

        public bool Accept(ExpPosition Pos, ImmutableStack<ASTNode> inputs, out ImmutableStack<ASTNode> results)
        {
            // A* A B 를 매치하는 방법
            // A A A A B

            // A A A A를 A*가 먹고, B를 먹어보기 x
            // A A A를 A*가 먹고, A를 A가 B를 B가 매치

            results = inputs;

            return true;
        }



    }
}
