﻿using Gum.Lang.AbstractSyntax;
using System.Collections.Generic;

namespace Gum.Translator.Text2AST.Parsing
{
    class NamespaceIDParser : Parser<NamespaceID>
    {
        protected override NamespaceID ParseInner(Lexer lexer)
        {
            var names = new List<string>();
            string name;

            // at least one namespace identifier                
            if (!lexer.Consume(TokenType.Identifier, out name)) throw new ParsingTokenFailedException<NamespaceIDParser>(TokenType.Identifier);
            names.Add(name);

            while (lexer.Consume(TokenType.Dot))
            {
                if (!lexer.Consume(TokenType.Identifier, out name)) throw new ParsingTokenFailedException<NamespaceIDParser>(TokenType.Identifier);
                names.Add(name);
            }

            return new NamespaceID(names);
        }
    }
}