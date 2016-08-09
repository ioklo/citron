using System;
using System.Runtime.Serialization;

namespace Gum.Translator.Text2AST.Parsing
{
    [Serializable]
    internal class ParingBinaryOperationFailedException : Exception
    {
        public ParingBinaryOperationFailedException()
        {
        }

        public ParingBinaryOperationFailedException(string message) : base(message)
        {
        }

        public ParingBinaryOperationFailedException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected ParingBinaryOperationFailedException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}