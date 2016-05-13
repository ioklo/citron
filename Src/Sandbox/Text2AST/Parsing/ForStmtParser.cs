using System;
using Gum.Lang.AbstractSyntax;

namespace Gum.Translator.Text2AST.Parsing
{
    class ForStmtParser : Parser<ForStmt>
    {
        protected override ForStmt ParseInner(Lexer lexer)
        {
            if (!lexer.Consume(TokenType.For))
                return null;

            if (!lexer.Consume(TokenType.LParen))
                throw new ParsingTokenFailedException<ForStmtParser>(TokenType.LParen);

            // 첫번째에 들어갈 수 있는 것들..
            // Nothing
            // Variable Declaration With Initial value
            // Expression

            IForInitComponent forInit = Parse<IForInitComponent, ForInitComponentParser>(lexer);
            if (!lexer.Consume(TokenType.SemiColon))
                throw new ParsingTokenFailedException<ForStmtParser>(TokenType.SemiColon);

            // 두번쨰에 들어갈 수 있는 것, IExp
            IExpComponent condExp = Parse<IExpComponent, ExpComponentParser>(lexer);

            if (!lexer.Consume(TokenType.SemiColon))
                throw new ParsingTokenFailedException<ForStmtParser>(TokenType.SemiColon);
            
            
            // 세번째에 들어갈 수 있는 것, IExp
            IExpComponent loopExp = Parse<IExpComponent, ExpComponentParser>(lexer);

            if (!lexer.Consume(TokenType.RParen))
                throw new ParsingTokenFailedException<ForStmtParser>(TokenType.RParen);

            IStmtComponent body = Parse<IStmtComponent, StmtComponentParser>(lexer);
            if (body == null)
                throw new ParsingFailedException<ForStmtParser, IStmtComponent>();

            return new ForStmt(forInit, condExp, loopExp, body);
        }

    }
}