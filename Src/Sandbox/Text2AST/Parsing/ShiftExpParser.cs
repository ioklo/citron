namespace Gum.Translator.Text2AST.Parsing
{
    // 5. << >>
    class ShiftExpParser : LeftAssocBinExpParser<AdditiveExpParser>
    {
        protected override TokenType[] OpTokenTypes
        {
            get
            {
                return new[] { TokenType.LessLess, TokenType.GreaterGreater };
            }
        }
    }
}