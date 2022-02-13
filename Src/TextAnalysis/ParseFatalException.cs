using System;
using System.Runtime.Serialization;

namespace Citron
{
    [Serializable]
    internal class ParseFatalException : Exception
    {
        public ParseFatalException()
        {
        }

        public ParseFatalException(string? message) : base(message)
        {
        }

        public ParseFatalException(string? message, Exception? innerException) : base(message, innerException)
        {
        }

        protected ParseFatalException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}