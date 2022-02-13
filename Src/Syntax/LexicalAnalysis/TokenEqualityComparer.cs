using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace Citron.LexicalAnalysis
{
    public class TokenEqualityComparer 
        : IEqualityComparer<Token>
    {
        public static readonly TokenEqualityComparer Instance = new TokenEqualityComparer();
        private TokenEqualityComparer() { }

        public bool Equals([AllowNull] Token x, [AllowNull] Token y)
        {
            switch((x, y))
            {
                case (null, null):
                    return true;

                case (IntToken intTokenX, IntToken intTokenY):
                    return intTokenX.Value == intTokenY.Value;

                case (BoolToken boolTokenX, BoolToken boolTokenY):
                    return boolTokenX.Value == boolTokenY.Value;

                case (TextToken textTokenX, TextToken textTokenY):
                    return textTokenX.Text == textTokenY.Text;

                case (IdentifierToken identifierTokenX, IdentifierToken identifierTokenY):
                    return identifierTokenX.Value == identifierTokenY.Value;

                default:
                    return ReferenceEquals(x, y);
            }
        }

        public int GetHashCode([DisallowNull] Token obj)
        {
            throw new NotImplementedException();
        }
    }
}
