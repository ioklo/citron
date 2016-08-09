using System;
using System.Runtime.Serialization;

namespace Gum.Translator.Text2AST.Parsing
{
    [Serializable]
    internal class ParsingExpFailedException<T1, T2> : Exception
    {
        public ParsingExpFailedException()
        {
        }

        public ParsingExpFailedException(string message) : base(message)
        {
        }

        public ParsingExpFailedException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected ParsingExpFailedException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}