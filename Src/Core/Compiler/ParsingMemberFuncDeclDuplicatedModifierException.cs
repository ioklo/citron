using System;
using System.Runtime.Serialization;

namespace Gum.Compiler
{
    [Serializable]
    class ParsingMemberFuncDeclDuplicatedModifierException : Exception
    {
        public ParsingMemberFuncDeclDuplicatedModifierException()
        {
        }

        public ParsingMemberFuncDeclDuplicatedModifierException(string message) : base(message)
        {
        }

        public ParsingMemberFuncDeclDuplicatedModifierException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected ParsingMemberFuncDeclDuplicatedModifierException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}