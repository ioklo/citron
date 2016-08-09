namespace Gum.Compiler
{
    partial class Parser
    {
        // 4. + - 
        class AdditiveExpParser : LeftAssocBinExpParser<MultiplicativeExpParser>
        {
            protected override TokenType[] OpTokenTypes
            {
                get
                {
                    return new[] { TokenType.Plus, TokenType.Minus };
                }
            }
        }
    }
}