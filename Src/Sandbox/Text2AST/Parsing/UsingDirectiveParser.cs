using System;
using Gum.Lang.AbstractSyntax;
using System.Collections.Generic;

namespace Gum.Translator.Text2AST.Parsing
{
    class UsingDirectiveParser : Parser<UsingDirective>
    {
        // using namespaceID.namespaceID;
        protected override UsingDirective ParseInner(Lexer lexer)
        {
            // first start with 'using' keyword
            if (!lexer.Consume(TokenType.Using)) return null;

            var namespaceID = Parse<NamespaceID, NamespaceIDParser>(lexer);
            if (namespaceID == null)
                throw new ParsingFailedException<UsingDirectiveParser, NamespaceID>();

            if (!lexer.Consume(TokenType.SemiColon))
                throw new ParsingTokenFailedException<UsingDirectiveParser>(TokenType.SemiColon);

            return new UsingDirective(namespaceID);
        }
    }
}