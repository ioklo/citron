namespace Gum.Compiler
{
    partial class Parser
    {
        // 6. Relational and type testing 
        // < > <= >=  // is as
        class RelationalAndTypeTestingExpParser : LeftAssocBinExpParser<ShiftExpParser>
        {
            protected override TokenType[] OpTokenTypes
            {
                get
                {
                    return new[]
                    {
                    TokenType.Less,
                    TokenType.Greater,
                    TokenType.LessEqual,
                    TokenType.GreaterEqual
                };
                }
            }
        }
    }
}