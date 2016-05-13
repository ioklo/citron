namespace Gum.Translator.Text2AST.Parsing
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