using System;
using System.Runtime.Serialization;

namespace Gum.Compiler
{
    partial class Parser
    {
        [Serializable]
        class ParsingExpFailedException<T1, T2> : Exception
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
}