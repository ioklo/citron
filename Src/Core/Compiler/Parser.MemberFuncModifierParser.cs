using Gum.Data.AbstractSyntax;

namespace Gum.Compiler
{
    partial class Parser
    {
        private MemberFuncModifier ParseMemberFuncModifier()
        {
            if (Consume(TokenType.Static))
                return MemberFuncModifier.Static;
            else if (Consume(TokenType.New))
                return MemberFuncModifier.New;
            else if (Consume(TokenType.Public))
                return MemberFuncModifier.Public;
            else if (Consume(TokenType.Protected))
                return MemberFuncModifier.Protected;
            else if (Consume(TokenType.Private))
                return MemberFuncModifier.Private;
            else if (Consume(TokenType.Virtual))
                return MemberFuncModifier.Virtual;
            else if (Consume(TokenType.Override))
                return MemberFuncModifier.Override;

            throw CreateException();
        } 
        
    }
}