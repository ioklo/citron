using Gum.Lang.AbstractSyntax;

namespace Gum.Translator.Text2AST.Parsing
{
    abstract class BinExpParser : Parser<IExpComponent>
    {
        protected BinaryExpKind? ConvertBinaryOperation(TokenType tokenType)
        {
            switch (tokenType)
            {
                case TokenType.AmperAmper: return BinaryExpKind.ConditionalAnd;
                case TokenType.BarBar: return BinaryExpKind.ConditionalOr;

                case TokenType.EqualEqual: return BinaryExpKind.Equal;
                case TokenType.NotEqual: return BinaryExpKind.NotEqual;

                case TokenType.Less: return BinaryExpKind.Less;
                case TokenType.LessEqual: return BinaryExpKind.LessEqual;
                case TokenType.Greater: return BinaryExpKind.Greater;
                case TokenType.GreaterEqual: return BinaryExpKind.GreaterEqual;
                case TokenType.Plus: return BinaryExpKind.Add;
                case TokenType.Minus: return BinaryExpKind.Sub;
                case TokenType.Star: return BinaryExpKind.Mul;
                case TokenType.Slash: return BinaryExpKind.Div;
                case TokenType.Percent: return BinaryExpKind.Mod;

                case TokenType.Equal: return BinaryExpKind.Assign;
                case TokenType.LessLess: return BinaryExpKind.ShiftLeft;
                case TokenType.GreaterGreater: return BinaryExpKind.ShiftRight;

            }

            return null;
        }
    }

}