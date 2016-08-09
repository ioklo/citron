using System;
using System.Runtime.Serialization;

namespace Gum.Translator.Text2AST.Parsing
{
    [Serializable]
    internal class ParsingExpTokenFailedException<T> : Exception
    {
        private TokenType rBracket;

        public ParsingExpTokenFailedException()
        {
        }

        public ParsingExpTokenFailedException(string message) : base(message)
        {
        }

        public ParsingExpTokenFailedException(TokenType rBracket)
        {
            this.rBracket = rBracket;
        }

        public ParsingExpTokenFailedException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected ParsingExpTokenFailedException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}