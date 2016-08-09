using System;
using Gum.Data.AbstractSyntax;

namespace Gum.Compiler
{
    partial class Parser
    {
        // 2. + - ! ~ ++x --x 
        // TODO: (T)x
        class UnaryExpParser : Parser<IExpComponent>
        {
            UnaryExpKind? ConvertPreUnaryOperation(TokenType tokenType)
            {
                switch (tokenType)
                {
                    case TokenType.PlusPlus: return UnaryExpKind.PrefixInc;
                    case TokenType.MinusMinus: return UnaryExpKind.PrefixDec;
                    case TokenType.Plus: return UnaryExpKind.Plus;
                    case TokenType.Minus: return UnaryExpKind.Minus;
                    case TokenType.Exclamation: return UnaryExpKind.Not;
                    case TokenType.Tilde: return UnaryExpKind.Neg;
                }

                return null;
            }

            protected override IExpComponent ParseInner(Lexer lexer)
            {
                TokenType tokenType;
                if (!lexer.ConsumeAny(out tokenType,
                        TokenType.Plus, TokenType.Minus,
                        TokenType.Exclamation, TokenType.Tilde, TokenType.PlusPlus, TokenType.MinusMinus))
                {
                    return Parse<IExpComponent, PrimaryExpParser>(lexer);
                }

                var unaryExpKind = ConvertPreUnaryOperation(tokenType);
                if (unaryExpKind == null)
                    throw new ParsingPreUnaryOperationFailedException();

                IExpComponent operandExp = Parse<IExpComponent, PrimaryExpParser>(lexer);
                if (operandExp == null)
                    throw new ParsingExpFailedException<UnaryExp, PrimaryExpParser>();

                return new UnaryExp(unaryExpKind.Value, operandExp);
            }
        }
    }
}