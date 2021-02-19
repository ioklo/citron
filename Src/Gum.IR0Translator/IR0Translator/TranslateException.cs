using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gum.IR0Translator
{
    class TranslateException : Exception
    {
        AnalyzeErrorCode code;
        Syntax.ISyntaxNode node;

        public TranslateException(AnalyzeErrorCode code, Syntax.ISyntaxNode node, string message)
            : base(message)
        {
            this.code = code;
            this.node = node;
        }
    }
}
