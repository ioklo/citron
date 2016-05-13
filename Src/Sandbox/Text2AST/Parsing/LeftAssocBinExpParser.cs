using Gum.Lang.AbstractSyntax;

namespace Gum.Translator.Text2AST.Parsing
{
    abstract class LeftAssocBinExpParser<ChildParser> : BinExpParser
        where ChildParser : Parser<IExpComponent>, new()
    {
        protected virtual TokenType[] OpTokenTypes { get; }

        protected override IExpComponent ParseInner(Lexer lexer)
        {
            IExpComponent leftExp = Parse<IExpComponent, ChildParser>(lexer);
            if (leftExp == null)
                throw new ParsingExpFailedException<ChildParser>();

            TokenType opTokenType;
            while (lexer.ConsumeAny(out opTokenType, OpTokenTypes))
            {
                IExpComponent rightExp = Parse<IExpComponent, ChildParser>(lexer);
                if (rightExp == null)
                    throw new ParsingExpFailedException<ChildParser>();

                var binOp = ConvertBinaryOperation(opTokenType);                
                leftExp = new BinaryExp(binOp.Value, leftExp, rightExp);
            }

            return leftExp;
        }
    }
}