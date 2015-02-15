using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gum.Translator.Parser
{
    public class ShiftStack
    {
        public Stack<ShiftStackItem> Stack { get; private set; }

        public ShiftStack()
        {
            Stack = new Stack<ShiftStackItem>();
        }

        public IEnumerable<Trace> GetReducibleTraces()
        {
            if (Stack.Count == 0) yield break;

            var curItem = Stack.Peek();
            foreach (var trace in curItem.Traces)
            {
                if (trace.IsReducible) yield return trace;
            }
        }

        public IEnumerable<Trace> GetAvailableTraces(Symbol symbol)
        {
            if (Stack.Count == 0) yield break;

            var curItem = Stack.Peek();

            foreach (var trace in curItem.Traces)
            {
                if (trace.IsAvailable(symbol)) yield return trace;
                else if (trace.IsReducible) yield return trace;
            }
        }

        public void Shift(IEnumerable<Trace> shiftableTraces, ASTNode curValue)
        {
            HashSet<Trace> nextTraces = new HashSet<Trace>();
            foreach(var trace in shiftableTraces)
            {
                foreach(var nextTrace in trace.Consume(curValue))
                {
                    nextTraces.Add(nextTrace);
                }
            }

            var item = new ShiftStackItem(nextTraces, curValue);
            Stack.Push(item);
        }

        internal bool Reduce(Trace trace, out ASTNode node)
        {
            return trace.Position.Rule.Reduce(this, trace, out node);
        }

        public ShiftStackItem Pop()
        {
            return Stack.Pop();
        }
    }
}
