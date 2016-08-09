namespace Gum.Compiler
{
    partial class Parser
    {
        // 9. ^
        class LogicalXORExpParser : LeftAssocBinExpParser<LogicalANDExpParser>
        {
            protected override TokenType[] OpTokenTypes { get { return new[] { TokenType.Caret }; } }
        }
    }
}