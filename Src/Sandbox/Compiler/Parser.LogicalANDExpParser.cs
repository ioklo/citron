namespace Gum.Compiler
{
    partial class Parser
    {
        // 8. &
        class LogicalANDExpParser : LeftAssocBinExpParser<EqualityExpParser>
        {
            protected override TokenType[] OpTokenTypes { get { return new[] { TokenType.Amper }; } }
        }
    }
}