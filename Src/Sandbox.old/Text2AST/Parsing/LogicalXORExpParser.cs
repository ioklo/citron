namespace Gum.Translator.Text2AST.Parsing
{
    // 9. ^
    internal class LogicalXORExpParser : LeftAssocBinExpParser<LogicalANDExpParser>
    {
        protected override TokenType[] OpTokenTypes { get { return new[] { TokenType.Caret }; } }
    }
}