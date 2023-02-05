using Citron.Log;
using System;
using System.Collections.Generic;
using S = Citron.Syntax;

namespace Citron.Analysis
{
    public class SyntaxAnalysisErrorLog : ILog
    {
        public SyntaxAnalysisErrorCode Code { get; }
        public S.ISyntaxNode Node { get; }
        public string Message { get; }

        public SyntaxAnalysisErrorLog(SyntaxAnalysisErrorCode code, S.ISyntaxNode node, string msg)
        {
            (Code, Node, Message) = (code, node, msg);
        }

        public override bool Equals(object? obj)
        {
            return obj is SyntaxAnalysisErrorLog log &&
                   Code == log.Code &&
                   EqualityComparer<S.ISyntaxNode>.Default.Equals(Node, log.Node);
                   // Message == log.Message; 메세지 비교 무시
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Code, Node, Message);
        }
    }
}
