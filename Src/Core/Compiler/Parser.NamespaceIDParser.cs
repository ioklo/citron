using Gum.Data.AbstractSyntax;
using System.Collections.Generic;

namespace Gum.Compiler
{
    partial class Parser
    {
        private NamespaceID ParseNamespaceID()
        {
            var names = new List<string>();
            string name;

            // at least one namespace identifier                
            ConsumeOrThrow(TokenType.Identifier, out name);
            names.Add(name);

            while (Consume(TokenType.Dot))
            {
                ConsumeOrThrow(TokenType.Identifier, out name);
                names.Add(name);
            }

            return new NamespaceID(names);
        }
    
    }
}