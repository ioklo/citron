using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gum.Translator.Parser
{
    public interface IRulePosition
    {
        Rule Rule { get; }

        bool IsAvailable(Symbol symbol);
        IEnumerable<IRulePosition> Consume(ASTNode node);

        bool IsReducible { get; }
        bool IsShiftable { get; }
    }
}
