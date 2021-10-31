using Gum.Log;
using Gum.Syntax;

namespace Gum.IR0Translator
{
    class TypeExpErrorLog : ILog
    {
        private TypeExpErrorCode code;
        private ISyntaxNode node;
        private string msg;

        public TypeExpErrorLog(TypeExpErrorCode code, ISyntaxNode node, string msg)
        {
            this.code = code;
            this.node = node;
            this.msg = msg;
        }

        string ILog.Message => msg;
    }
}