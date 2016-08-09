using System;
using System.Runtime.Serialization;

namespace Gum.Compiler
{
    [Serializable]
    class ParsingThisExpFailedException : Exception
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