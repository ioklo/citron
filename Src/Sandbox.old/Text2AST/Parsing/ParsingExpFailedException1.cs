using System;
using System.Runtime.Serialization;
using Gum.Lang.AbstractSyntax;

namespace Gum.Translator.Text2AST.Parsing
{
    [Serializable]
    internal class ParsingExpFailedException<ChildParser> : Exception where ChildParser : Parser<IExpComponent>, new()
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