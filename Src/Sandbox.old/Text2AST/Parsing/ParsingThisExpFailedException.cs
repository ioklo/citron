using System;
using System.Runtime.Serialization;

namespace Gum.Translator.Text2AST.Parsing
{
    [Serializable]
    internal class ParsingThisExpFailedException : Exception
    {
        public ParsingThisExpFailedException()
        {
        }

        public ParsingThisExpFailedException(string message) : base(message)
        {
        }

        public ParsingThisExpFailedException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected ParsingThisExpFailedException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}