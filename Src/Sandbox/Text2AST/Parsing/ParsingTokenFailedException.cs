using System;
using System.Runtime.Serialization;

namespace Gum.Translator.Text2AST.Parsing
{
    [Serializable]
    internal class ParsingTokenFailedException<T> : Exception
    {
        private TokenType tokenType;

        public ParsingTokenFailedException()
        {
        }

        public ParsingTokenFailedException(string message) : base(message)
        {
        }

        public ParsingTokenFailedException(TokenType tokenType)
        {
            this.tokenType = tokenType;
        }

        public ParsingTokenFailedException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected ParsingTokenFailedException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}