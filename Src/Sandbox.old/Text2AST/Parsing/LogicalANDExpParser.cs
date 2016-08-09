namespace Gum.Translator.Text2AST.Parsing
{
    // 8. &
    class LogicalANDExpParser : LeftAssocBinExpParser<EqualityExpParser>
    {
        protected override TokenType[] OpTokenTypes { get { return new[] { TokenType.Amper }; } }
    }
}