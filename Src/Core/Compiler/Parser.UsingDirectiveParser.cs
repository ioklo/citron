using System;
using Gum.Data.AbstractSyntax;
using System.Collections.Generic;

namespace Gum.Compiler
{
    partial class Parser
    {
        // using namespaceID.namespaceID;
        private UsingDirective ParseUsingDirective(Lexer lexer)
        {
            // first start with 'using' keyword
            if (!lexer.Consume(TokenType.Using)) return null;
            var namespaceID = ParseNamespaceID();
            ConsumeOrThrow(TokenType.SemiColon);

            return new UsingDirective(namespaceID);
        }
        
    }
}