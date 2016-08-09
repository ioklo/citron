using System;
using System.Runtime.Serialization;

namespace Gum.Compiler
{
    [Serializable]
    class ParsingBinaryOperationFailedException : Exception
    {
        public ParsingBinaryOperationFailedException()
        {
        }

        public ParsingBinaryOperationFailedException(string message) : base(message)
        {
        }

        public ParsingBinaryOperationFailedException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected ParsingBinaryOperationFailedException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}