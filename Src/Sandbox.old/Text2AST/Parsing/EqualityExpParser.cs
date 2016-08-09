namespace Gum.Translator.Text2AST.Parsing
{
    // 7. == !=
    class EqualityExpParser : LeftAssocBinExpParser<RelationalAndTypeTestingExpParser>
    {
        protected override TokenType[] OpTokenTypes
        {
            get
            {
                return new[] { TokenType.EqualEqual, TokenType.NotEqual };
            }
        }
    }
}