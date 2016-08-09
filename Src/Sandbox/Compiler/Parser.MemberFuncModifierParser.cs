using Gum.Data.AbstractSyntax;

namespace Gum.Compiler
{
    partial class Parser
    {
        class MemberFuncModifierParser : Parser<MemberFuncModifier?>
        {
            // TokenType -> MemberFuncModifier

            protected override MemberFuncModifier? ParseInner(Lexer lexer)
            {
                if (lexer.Consume(TokenType.Static))
                    return MemberFuncModifier.Static;
                else if (lexer.Consume(TokenType.New))
                    return MemberFuncModifier.New;
                else if (lexer.Consume(TokenType.Public))
                    return MemberFuncModifier.Public;
                else if (lexer.Consume(TokenType.Protected))
                    return MemberFuncModifier.Protected;
                else if (lexer.Consume(TokenType.Private))
                    return MemberFuncModifier.Private;
                else if (lexer.Consume(TokenType.Virtual))
                    return MemberFuncModifier.Virtual;
                else if (lexer.Consume(TokenType.Override))
                    return MemberFuncModifier.Override;

                return null;
            }
        }
    }
}