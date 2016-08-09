namespace Gum.Compiler
{
    partial class Parser
    {
        // 11. &&
        class ConditionalANDExpParser : LeftAssocBinExpParser<LogicalORExpParser>
        {
            private TokenType[] tokenTypes = new[] { TokenType.AmperAmper };
            protected override TokenType[] OpTokenTypes { get { return tokenTypes; } }
        }
    }
}