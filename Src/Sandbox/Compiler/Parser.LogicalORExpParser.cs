namespace Gum.Compiler
{
    partial class Parser
    {
        // 10. |
        class LogicalORExpParser : LeftAssocBinExpParser<LogicalXORExpParser>
        {
            private TokenType[] tokenTypes = new[] { TokenType.Bar };
            protected override TokenType[] OpTokenTypes { get { return tokenTypes; } }
        }
    }
}