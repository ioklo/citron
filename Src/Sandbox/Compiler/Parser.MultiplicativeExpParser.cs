namespace Gum.Compiler
{
    partial class Parser
    {
        // 3. * / %
        class MultiplicativeExpParser : LeftAssocBinExpParser<UnaryExpParser>
        {
            protected override TokenType[] OpTokenTypes
            {
                get
                {
                    return new[] { TokenType.Star, TokenType.Slash, TokenType.Percent };
                }
            }
        }
    }
}