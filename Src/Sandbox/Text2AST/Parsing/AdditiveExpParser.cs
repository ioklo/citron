namespace Gum.Translator.Text2AST.Parsing
{
    // 4. + - 
    internal class AdditiveExpParser : LeftAssocBinExpParser<MultiplicativeExpParser>
    {
        protected override TokenType[] OpTokenTypes
        {
            get
            {
                return new[] { TokenType.Plus, TokenType.Minus };
            }
        }
    }

}