using System;
using System.Runtime.Serialization;

namespace Gum.Compiler
{
    [Serializable]
    class ParsingFailedException<T1, T2> : Exception
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