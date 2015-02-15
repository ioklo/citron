using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;

namespace Gum.Translator.Parser
{
    public class ShiftStackItem
    {
        // Has TraceSet And ASTNode
        public IEnumerable<Trace> Traces { get { return traces; } }
        public ASTNode Node { get; private set; }

        private HashSet<Trace> traces = new HashSet<Trace>();        
        
        public ShiftStackItem(HashSet<Trace> traces, ASTNode node)
        {
            this.traces = traces;
            this.Node = node;
        }
    }
}
