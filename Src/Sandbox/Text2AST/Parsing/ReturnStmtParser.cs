using Gum.Lang.AbstractSyntax;
using Gum.Translator.Text2AST.Parsing;
using System;

namespace Gum.Translator.Text2AST.Parsing
{
    internal class ReturnStmtParser : Parser<ReturnStmt>
    {
        protected override ReturnStmt ParseInner(Lexer lexer)
        {
            if (!lexer.Consume(TokenType.Return))
                return null;

            if (lexer.Consume(TokenType.SemiColon))
                return new ReturnStmt(null);

            IExpComponent exp = Parse<IExpComponent, ExpComponentParser>(lexer);
            if (exp == null)
                throw new ParsingFailedException<ReturnStmtParser, IExpComponent>();

            if (!lexer.Consume(TokenType.SemiColon))
                throw new ParsingTokenFailedException<ReturnStmtParser>(TokenType.SemiColon);

            return new ReturnStmt(exp);
        }

    }
}