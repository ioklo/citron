﻿using Citron.Log;
using Citron.Syntax;

namespace Citron.Analysis
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