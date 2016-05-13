using System;
using System.Runtime.Serialization;

namespace Gum.Translator.Text2AST.Parsing
{
    [Serializable]
    internal class ParsingFileUnitComponentFailedException : Exception
    {
        public ParsingFileUnitComponentFailedException()
            : this("파일단위 구성요소를 파싱하지 못했습니다. Using지시자 거나 네임스페이스 선언이어야 합니다.")
        {
        }

        public ParsingFileUnitComponentFailedException(string message) : base(message)
        {
        }

        public ParsingFileUnitComponentFailedException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected ParsingFileUnitComponentFailedException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}