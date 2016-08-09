using System;
using System.Runtime.Serialization;

namespace Gum.Translator.Text2AST.Parsing
{
    [Serializable]
    internal class ParsingPrimarySingleExpFailed : Exception
    {
        public ParsingPrimarySingleExpFailed()
        {
        }

        public ParsingPrimarySingleExpFailed(string message) : base(message)
        {
        }

        public ParsingPrimarySingleExpFailed(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected ParsingPrimarySingleExpFailed(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}