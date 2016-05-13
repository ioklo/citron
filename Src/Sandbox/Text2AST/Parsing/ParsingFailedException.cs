using System;
using System.Runtime.Serialization;

namespace Gum.Translator.Text2AST.Parsing
{
    [Serializable]
    internal class ParsingFailedException<T1, T2> : Exception
    {
        public ParsingFailedException()
        {
        }

        public ParsingFailedException(string message) : base(message)
        {
        }

        public ParsingFailedException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected ParsingFailedException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}