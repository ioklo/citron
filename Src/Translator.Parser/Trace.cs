using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gum.Translator.Parser
{
    public class Trace
    {
        public Trace PrevTrace { get; private set; }
        public IRulePosition Position { get; private set; }

        public Trace(Trace prevTrace, IRulePosition position)
        {
            PrevTrace = prevTrace;
            Position = position;
        }
        
        // 이 Trace에서 
        public bool IsAvailable(Symbol symbol)
        {
            return Position.IsAvailable(symbol);
        }

        internal IEnumerable<Trace> Consume(ASTNode node)
        {
            foreach( var pos in Position.Consume(node))
            {
                yield return new Trace(this, pos);
            }
        }

        public bool IsShiftable
        {
            get
            {
                return Position.IsShiftable;

            }
        }

        public bool IsReducible
        {
            get
            {
                return Position.IsReducible;
            }
        }

        public static bool Compare(Trace trace1, Trace trace2, out int result)
        {
            // 비교할 수 있다면 true, 없다면 false
            // trace1.Position.Rule
            // 비교할 수 없다.. 뭐로 reduce될지 알 수 없기 때문이다.

            // Add가 Exp와 S로 Reduce될 수 있다면
            // Mul은 S로만 Reduce될 수 있다면

            // nondeterministic finite machine을 상상한다
            // 각자 자기만의 고유한 스택이 있어야 한다

            // Add -> E 와
            // Mul을 비교해야 하는가?
            throw new Exception();
        }

        public override string ToString()
        {
            if( PrevTrace == null )
            {
                return Position.ToString();
            }
            else
            {
                return PrevTrace.ToString() + ", " + Position.ToString();
            }
        }

        
    }
}
